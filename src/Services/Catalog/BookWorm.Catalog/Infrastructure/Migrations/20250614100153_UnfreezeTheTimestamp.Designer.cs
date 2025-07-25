﻿// <auto-generated />
using System;
using BookWorm.Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookWorm.Catalog.Infrastructure.Migrations
{
    [DbContext(typeof(CatalogDbContext))]
    [Migration("20250614100153_UnfreezeTheTimestamp")]
    partial class UnfreezeTheTimestamp
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate.Author", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_authors");

                    b.ToTable("authors", (string)null);
                });

            modelBuilder.Entity("BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Book", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<double>("AverageRating")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("double precision")
                        .HasDefaultValue(0.0)
                        .HasColumnName("average_rating");

                    b.Property<Guid?>("CategoryId")
                        .HasColumnType("uuid")
                        .HasColumnName("category_id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)")
                        .HasColumnName("description");

                    b.Property<string>("Image")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("image");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<DateTime?>("LastModifiedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_modified_at")
                        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("name");

                    b.Property<Guid?>("PublisherId")
                        .HasColumnType("uuid")
                        .HasColumnName("publisher_id");

                    b.Property<byte>("Status")
                        .HasColumnType("smallint")
                        .HasColumnName("status");

                    b.Property<int>("TotalReviews")
                        .HasColumnType("integer")
                        .HasColumnName("total_reviews");

                    b.Property<Guid>("Version")
                        .IsConcurrencyToken()
                        .HasColumnType("uuid")
                        .HasColumnName("version");

                    b.HasKey("Id")
                        .HasName("pk_books");

                    b.HasIndex("CategoryId")
                        .HasDatabaseName("ix_books_category_id");

                    b.HasIndex("PublisherId")
                        .HasDatabaseName("ix_books_publisher_id");

                    b.ToTable("books", (string)null);
                });

            modelBuilder.Entity("BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.BookAuthor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("AuthorId")
                        .HasColumnType("uuid")
                        .HasColumnName("author_id");

                    b.Property<Guid>("BookId")
                        .HasColumnType("uuid")
                        .HasColumnName("book_id");

                    b.HasKey("Id")
                        .HasName("pk_book_authors");

                    b.HasIndex("AuthorId")
                        .HasDatabaseName("ix_book_authors_author_id");

                    b.HasIndex("BookId")
                        .HasDatabaseName("ix_book_authors_book_id");

                    b.ToTable("book_authors", (string)null);
                });

            modelBuilder.Entity("BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_categories");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_categories_name");

                    b.ToTable("categories", (string)null);
                });

            modelBuilder.Entity("BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate.Publisher", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_publishers");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_publishers_name");

                    b.ToTable("publishers", (string)null);
                });

            modelBuilder.Entity("BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Book", b =>
                {
                    b.HasOne("BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_books_categories_category_id");

                    b.HasOne("BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate.Publisher", "Publisher")
                        .WithMany()
                        .HasForeignKey("PublisherId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_books_publishers_publisher_id");

                    b.OwnsOne("BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Price", "Price", b1 =>
                        {
                            b1.Property<Guid>("BookId")
                                .HasColumnType("uuid")
                                .HasColumnName("id");

                            b1.Property<decimal?>("DiscountPrice")
                                .HasPrecision(18, 2)
                                .HasColumnType("numeric(18,2)")
                                .HasColumnName("price_discount_price");

                            b1.Property<decimal>("OriginalPrice")
                                .HasPrecision(18, 2)
                                .HasColumnType("numeric(18,2)")
                                .HasColumnName("price_original_price");

                            b1.HasKey("BookId");

                            b1.ToTable("books");

                            b1.WithOwner()
                                .HasForeignKey("BookId")
                                .HasConstraintName("fk_books_books_id");
                        });

                    b.Navigation("Category");

                    b.Navigation("Price");

                    b.Navigation("Publisher");
                });

            modelBuilder.Entity("BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.BookAuthor", b =>
                {
                    b.HasOne("BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate.Author", "Author")
                        .WithMany("BookAuthors")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_book_authors_authors_author_id");

                    b.HasOne("BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Book", "Book")
                        .WithMany("BookAuthors")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_book_authors_books_book_id");

                    b.Navigation("Author");

                    b.Navigation("Book");
                });

            modelBuilder.Entity("BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate.Author", b =>
                {
                    b.Navigation("BookAuthors");
                });

            modelBuilder.Entity("BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Book", b =>
                {
                    b.Navigation("BookAuthors");
                });
#pragma warning restore 612, 618
        }
    }
}
