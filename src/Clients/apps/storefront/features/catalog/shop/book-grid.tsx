"use client";

import Link from "next/link";

import type { Book } from "@workspace/types/catalog/books";
import { Button } from "@workspace/ui/components/button";

import { BookCard } from "@/components/book-card";
import { BookCardSkeleton } from "@/components/loading-skeleton";
import { Pagination } from "@/components/pagination";

const SKELETON_KEYS = Array.from({ length: 8 }, (_, i) => `skeleton-${i}`);

type BookGridProps = {
  books: Book[];
  isLoading: boolean;
  totalPages: number;
  currentPage: number;
  onPageChange: (page: number) => void;
  onClearFilters: () => void;
};

export default function BookGrid({
  books,
  isLoading,
  totalPages,
  currentPage,
  onPageChange,
  onClearFilters,
}: Readonly<BookGridProps>) {
  if (isLoading) {
    return (
      <output
        className="mb-12 grid grid-cols-2 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
        aria-label="Loading books"
      >
        {SKELETON_KEYS.map((key) => (
          <BookCardSkeleton key={key} />
        ))}
      </output>
    );
  }

  if (books.length === 0) {
    return (
      <section
        className="flex flex-col items-center justify-center py-24 text-center"
        aria-labelledby="no-results"
      >
        <h2 id="no-results" className="mb-4 font-serif text-3xl font-medium">
          No Books Found
        </h2>
        <p className="text-muted-foreground mb-8 max-w-md">
          We couldn't find any books matching your filters. Try adjusting your
          selection or browse all our titles.
        </p>
        <Button
          type="button"
          variant="outline"
          onClick={onClearFilters}
          className="rounded-full bg-transparent"
          aria-label="Clear all filters"
        >
          Clear Filters
        </Button>
      </section>
    );
  }

  return (
    <>
      <ul
        className="mb-12 grid grid-cols-2 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
        aria-label="Book catalog"
      >
        {books.map((book) => (
          <li key={book.id}>
            <Link href={`/shop/${book.id}`}>
              <BookCard book={book} />
            </Link>
          </li>
        ))}
      </ul>

      {totalPages > 1 && (
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={onPageChange}
        />
      )}
    </>
  );
}
