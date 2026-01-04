"use client";

import { useRouter } from "next/navigation";

import { ArrowRight } from "lucide-react";

import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import { Button } from "@workspace/ui/components/button";

import { BookCard } from "@/components/book-card";
import { BookCardSkeleton } from "@/components/loading-skeleton";

export default function FeaturedBooksSection() {
  const router = useRouter();

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
        {hasFeaturedBooks && (
          <Button
            type="button"
            variant="ghost"
            className="hidden gap-2 md:flex"
            onClick={() => router.push("/shop")}
            aria-label="View all books"
          >
            View All <ArrowRight className="size-4" aria-hidden="true" />
          </Button>
        )}
      </div>

      <div className="grid grid-cols-2 gap-8 sm:grid-cols-2 lg:grid-cols-4">
        {isLoading
          ? Array.from({ length: 4 }).map((_, i) => (
              <BookCardSkeleton key={i} />
            ))
          : books.map((book) => (
              <BookCard
                key={book.id}
                book={book}
                onClick={() => router.push(`/shop/${book.id}`)}
              />
            ))}
      </div>
    </section>
  );
}
