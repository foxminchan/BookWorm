CREATE TABLE authors (
    id uuid NOT NULL DEFAULT uuidv7(),
    name character varying(100) NOT NULL,
    CONSTRAINT pk_authors PRIMARY KEY (id)
);

CREATE TABLE categories (
    id uuid NOT NULL DEFAULT uuidv7(),
    name character varying(50) NOT NULL,
    CONSTRAINT pk_categories PRIMARY KEY (id)
);

CREATE TABLE publishers (
    id uuid NOT NULL DEFAULT uuidv7(),
    name character varying(100) NOT NULL,
    CONSTRAINT pk_publishers PRIMARY KEY (id)
);

CREATE TABLE books (
    id uuid NOT NULL DEFAULT uuidv7(),
    name character varying(50) NOT NULL,
    description character varying(1000) NOT NULL,
    image character varying(255),
    price_original_price numeric(18, 2),
    price_discount_price numeric(18, 2),
    status smallint NOT NULL,
    average_rating double precision NOT NULL DEFAULT 0.0,
    total_reviews integer NOT NULL,
    category_id uuid,
    publisher_id uuid,
    is_deleted boolean NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_modified_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_books PRIMARY KEY (id),
    CONSTRAINT fk_books_categories_category_id
        FOREIGN KEY (category_id) REFERENCES categories (id) ON DELETE SET NULL,
    CONSTRAINT fk_books_publishers_publisher_id
        FOREIGN KEY (publisher_id) REFERENCES publishers (id) ON DELETE SET NULL
);

CREATE TABLE book_authors (
    id uuid NOT NULL DEFAULT uuidv7(),
    author_id uuid NOT NULL,
    book_id uuid NOT NULL,
    CONSTRAINT pk_book_authors PRIMARY KEY (id),
    CONSTRAINT fk_book_authors_authors_author_id
        FOREIGN KEY (author_id) REFERENCES authors (id) ON DELETE RESTRICT,
    CONSTRAINT fk_book_authors_books_book_id
        FOREIGN KEY (book_id) REFERENCES books (id) ON DELETE CASCADE
);

CREATE INDEX ix_book_authors_author_id ON book_authors (author_id);
CREATE INDEX ix_book_authors_book_id ON book_authors (book_id);
CREATE INDEX ix_books_category_id ON books (category_id);
CREATE INDEX ix_books_publisher_id ON books (publisher_id);
CREATE UNIQUE INDEX ix_categories_name ON categories (name);
CREATE UNIQUE INDEX ix_publishers_name ON publishers (name);

-- Books waiting for asynchronous vector indexing.
CREATE TABLE pending_vector_updates (
    book_id uuid PRIMARY KEY,
    revision uuid NOT NULL DEFAULT gen_random_uuid(),
    updated_at timestamp with time zone NOT NULL DEFAULT clock_timestamp()
);

CREATE INDEX ix_pending_vector_updates_updated_at
    ON pending_vector_updates (updated_at);

CREATE FUNCTION enqueue_vector_update(target_book_id uuid) RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO pending_vector_updates (book_id)
    VALUES (target_book_id)
    ON CONFLICT (book_id) DO UPDATE
    SET revision = gen_random_uuid(),
        updated_at = clock_timestamp();
$$;

CREATE FUNCTION enqueue_book_vector_update() RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF TG_OP = 'DELETE' THEN
        PERFORM enqueue_vector_update(OLD.id);
        RETURN OLD;
    END IF;

    PERFORM enqueue_vector_update(NEW.id);
    RETURN NEW;
END;
$$;

CREATE TRIGGER books_vector_update
AFTER INSERT OR UPDATE OR DELETE ON books
FOR EACH ROW
EXECUTE FUNCTION enqueue_book_vector_update();

CREATE FUNCTION enqueue_book_author_vector_update() RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF TG_OP <> 'INSERT' THEN
        PERFORM enqueue_vector_update(OLD.book_id);
    END IF;

    IF TG_OP <> 'DELETE' THEN
        PERFORM enqueue_vector_update(NEW.book_id);
        RETURN NEW;
    END IF;

    RETURN OLD;
END;
$$;

CREATE TRIGGER book_authors_vector_update
AFTER INSERT OR UPDATE OR DELETE ON book_authors
FOR EACH ROW
EXECUTE FUNCTION enqueue_book_author_vector_update();

CREATE FUNCTION enqueue_related_book_vector_updates() RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF TG_TABLE_NAME = 'authors' THEN
        PERFORM enqueue_vector_update(book_id)
        FROM book_authors
        WHERE author_id = OLD.id;
    ELSIF TG_TABLE_NAME = 'categories' THEN
        PERFORM enqueue_vector_update(id)
        FROM books
        WHERE category_id = OLD.id;
    ELSIF TG_TABLE_NAME = 'publishers' THEN
        PERFORM enqueue_vector_update(id)
        FROM books
        WHERE publisher_id = OLD.id;
    END IF;

    IF TG_OP = 'DELETE' THEN
        RETURN OLD;
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER authors_vector_update
BEFORE UPDATE OR DELETE ON authors
FOR EACH ROW
EXECUTE FUNCTION enqueue_related_book_vector_updates();

CREATE TRIGGER categories_vector_update
BEFORE UPDATE OR DELETE ON categories
FOR EACH ROW
EXECUTE FUNCTION enqueue_related_book_vector_updates();

CREATE TRIGGER publishers_vector_update
BEFORE UPDATE OR DELETE ON publishers
FOR EACH ROW
EXECUTE FUNCTION enqueue_related_book_vector_updates();

-- Existing books must be indexed by the first vector-ingestion batches.
INSERT INTO pending_vector_updates (book_id)
SELECT id
FROM books;
