using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookWorm.Catalog.Infrastructure.Migrations;

// The database maintains this queue atomically with catalog changes; EF does not track it.
[DbContext(typeof(CatalogDbContext))]
[Migration("20261004000000_AddPendingVectorUpdates")]
public sealed class AddPendingVectorUpdates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TABLE pending_vector_updates (
                book_id uuid PRIMARY KEY,
                revision uuid NOT NULL DEFAULT gen_random_uuid(),
                updated_at timestamptz NOT NULL DEFAULT clock_timestamp()
            );
            CREATE INDEX ix_pending_vector_updates_updated_at ON pending_vector_updates (updated_at);

            CREATE FUNCTION enqueue_vector_update(target_book_id uuid) RETURNS void
            LANGUAGE sql AS $$
                INSERT INTO pending_vector_updates (book_id) VALUES (target_book_id)
                ON CONFLICT (book_id) DO UPDATE
                SET revision = gen_random_uuid(), updated_at = clock_timestamp();
            $$;

            CREATE FUNCTION enqueue_book_vector_update() RETURNS trigger
            LANGUAGE plpgsql AS $$
            BEGIN
                IF TG_OP = 'DELETE' THEN
                    PERFORM enqueue_vector_update(OLD.id);
                    RETURN OLD;
                END IF;
                PERFORM enqueue_vector_update(NEW.id);
                RETURN NEW;
            END;
            $$;
            CREATE TRIGGER books_vector_update AFTER INSERT OR UPDATE OR DELETE ON books
                FOR EACH ROW EXECUTE FUNCTION enqueue_book_vector_update();

            CREATE FUNCTION enqueue_book_author_vector_update() RETURNS trigger
            LANGUAGE plpgsql AS $$
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
            CREATE TRIGGER book_authors_vector_update AFTER INSERT OR UPDATE OR DELETE ON book_authors
                FOR EACH ROW EXECUTE FUNCTION enqueue_book_author_vector_update();

            CREATE FUNCTION enqueue_related_book_vector_updates() RETURNS trigger
            LANGUAGE plpgsql AS $$
            BEGIN
                IF TG_TABLE_NAME = 'authors' THEN
                    PERFORM enqueue_vector_update(book_id) FROM book_authors WHERE author_id = OLD.id;
                ELSIF TG_TABLE_NAME = 'categories' THEN
                    PERFORM enqueue_vector_update(id) FROM books WHERE category_id = OLD.id;
                ELSIF TG_TABLE_NAME = 'publishers' THEN
                    PERFORM enqueue_vector_update(id) FROM books WHERE publisher_id = OLD.id;
                END IF;
                IF TG_OP = 'DELETE' THEN
                    RETURN OLD;
                END IF;
                RETURN NEW;
            END;
            $$;
            CREATE TRIGGER authors_vector_update BEFORE UPDATE OR DELETE ON authors
                FOR EACH ROW EXECUTE FUNCTION enqueue_related_book_vector_updates();
            CREATE TRIGGER categories_vector_update BEFORE UPDATE OR DELETE ON categories
                FOR EACH ROW EXECUTE FUNCTION enqueue_related_book_vector_updates();
            CREATE TRIGGER publishers_vector_update BEFORE UPDATE OR DELETE ON publishers
                FOR EACH ROW EXECUTE FUNCTION enqueue_related_book_vector_updates();

            -- Refresh the existing index in the first scheduled batches.
            INSERT INTO pending_vector_updates (book_id) SELECT id FROM books;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP TRIGGER publishers_vector_update ON publishers;
            DROP TRIGGER categories_vector_update ON categories;
            DROP TRIGGER authors_vector_update ON authors;
            DROP TRIGGER book_authors_vector_update ON book_authors;
            DROP TRIGGER books_vector_update ON books;
            DROP FUNCTION enqueue_related_book_vector_updates();
            DROP FUNCTION enqueue_book_author_vector_update();
            DROP FUNCTION enqueue_book_vector_update();
            DROP FUNCTION enqueue_vector_update(uuid);
            DROP TABLE pending_vector_updates;
            """
        );
    }
}
