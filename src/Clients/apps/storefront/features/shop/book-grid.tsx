"use client";

import Link from "next/link";
import { Button } from "@workspace/ui/components/button";
import { BookCard } from "@/components/book-card";
import { BookCardSkeleton } from "@/components/loading-skeleton";
import { Pagination } from "@/components/pagination";

type BookGridProps = {
  books: any[];
  isLoading: boolean;
  totalPages: number;
  currentPage: number;
  onPageChange: (page: number) => void;
  onClearFilters: () => void;
};

export function BookGrid({
  books,
  isLoading,
  totalPages,
  currentPage,
  onPageChange,
  onClearFilters,
}: BookGridProps) {
  if (isLoading) {
    return (
      <div
        className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 mb-12"
        role="status"
        aria-label="Loading books"
      >
        {Array.from({ length: 8 }).map((_: any, i: number) => (
          <BookCardSkeleton key={i} />
        ))}
      </div>
    );
  }

  if (books.length === 0) {
    return (
      <div
        className="flex flex-col items-center justify-center py-24 text-center"
        role="region"
        aria-labelledby="no-results"
      >
        <h2 id="no-results" className="text-3xl font-serif font-medium mb-4">
          No Books Found
        </h2>
        <p className="text-muted-foreground mb-8 max-w-md">
          We couldn't find any books matching your filters. Try adjusting your
          selection or browse all our titles.
        </p>
        <Button
          variant="outline"
          onClick={onClearFilters}
          className="rounded-full bg-transparent"
        >
          Clear Filters
        </Button>
      </div>
    );
  }

  return (
    <>
      <div
        className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 mb-12"
        role="list"
      >
        {books.map((book: any) => (
          <Link key={book.id} href={`/shop/${book.id}`}>
            <BookCard book={book} />
          </Link>
        ))}
      </div>

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
