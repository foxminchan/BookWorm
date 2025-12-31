"use client";

import { Button } from "@workspace/ui/components/button";
import { ArrowRight } from "lucide-react";
import { BookCardSkeleton } from "@/components/loading-skeleton";
import { BookCard } from "@/components/book-card";
import { useRouter } from "next/navigation";
import type { Book } from "@workspace/types/catalog/books";

type FeaturedBooksSectionProps = {
  books: Book[];
  isLoading: boolean;
};

export default function FeaturedBooksSection({
  books,
  isLoading,
}: FeaturedBooksSectionProps) {
  const router = useRouter();
  const hasFeaturedBooks = books.length > 0;

  return (
    <section
      className="py-24 container mx-auto px-4"
      aria-labelledby="featured-heading"
    >
      <div className="flex items-end justify-between mb-12">
        <div>
          <h2
            id="featured-heading"
            className="text-3xl font-serif font-medium mb-2"
          >
            Featured Books
          </h2>
          <p className="text-muted-foreground">
            The most anticipated titles of the season.
          </p>
        </div>
        {hasFeaturedBooks && (
          <Button
            variant="ghost"
            className="hidden md:flex gap-2"
            onClick={() => router.push("/shop")}
          >
            View All <ArrowRight className="size-4" />
          </Button>
        )}
      </div>

      <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-8">
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
