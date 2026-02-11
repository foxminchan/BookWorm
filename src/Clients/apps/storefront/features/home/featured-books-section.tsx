"use client";

import Link from "next/link";

import { ArrowRight } from "lucide-react";

import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import { Button } from "@workspace/ui/components/button";

import { BookCard } from "@/components/book-card";
import { BookCardSkeleton } from "@/components/loading-skeleton";

export default function FeaturedBooksSection() {
  // This will use the hydrated data from the server
  const { data: booksData, isLoading } = useBooks({ pageSize: 4 });

  const books = Array.isArray(booksData?.items)
    ? booksData.items.slice(0, 4)
    : [];
  const hasFeaturedBooks = books.length > 0;

  return (
    <section
      className="container mx-auto px-4 py-24"
      aria-labelledby="featured-heading"
    >
      <div className="mb-12 flex items-end justify-between">
        <div>
          <h2
            id="featured-heading"
            className="mb-2 font-serif text-3xl font-medium"
          >
            Featured Books
          </h2>
          <p className="text-muted-foreground">
            The most anticipated titles of the season.
          </p>
        </div>
        {hasFeaturedBooks ? (
          <Button
            variant="ghost"
            className="hidden gap-2 md:flex"
            aria-label="View all books"
            asChild
          >
            <Link href="/shop">
              View All <ArrowRight className="size-4" aria-hidden="true" />
            </Link>
          </Button>
        ) : null}
      </div>

      <div className="grid grid-cols-2 gap-8 sm:grid-cols-2 lg:grid-cols-4">
        {isLoading
          ? Array.from({ length: 4 }).map((_, i) => (
              <BookCardSkeleton key={`skeleton-${i.toString()}`} />
            ))
          : books.map((book) => (
              <Link key={book.id} href={`/shop/${book.id}`}>
                <BookCard book={book} />
              </Link>
            ))}
      </div>
    </section>
  );
}
