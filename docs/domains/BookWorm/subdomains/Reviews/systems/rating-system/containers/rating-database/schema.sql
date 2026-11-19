CREATE TABLE feedbacks (
    id uuid NOT NULL DEFAULT uuidv7(),
    first_name character varying(50) NOT NULL,
    last_name character varying(50) NOT NULL,
    comment character varying(1000),
    rating integer NOT NULL,
    created_at timestamp with time zone NOT NULL,
    last_modified_at timestamp with time zone,
    book_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    CONSTRAINT pk_feedbacks PRIMARY KEY (id)
);

CREATE UNIQUE INDEX ix_feedbacks_book_id_first_name_last_name
    ON feedbacks (book_id, first_name, last_name);
