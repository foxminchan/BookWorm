BEGIN;

CREATE TABLE IF NOT EXISTS public."Conversations"
(
    "Id" uuid NOT NULL DEFAULT uuidv7(),
    "Name" character varying(100) COLLATE pg_catalog."default" NOT NULL,
    "UserId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "Messages" jsonb,
    CONSTRAINT "PK_Conversations" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS public."__EFMigrationsHistory"
(
    "MigrationId" character varying(150) COLLATE pg_catalog."default" NOT NULL,
    "ProductVersion" character varying(32) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

END;
