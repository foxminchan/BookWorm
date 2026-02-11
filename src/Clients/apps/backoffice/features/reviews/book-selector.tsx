"use client";

import { useMemo } from "react";

import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";

import { DEFAULT_BOOKS_PAGE_SIZE } from "@/lib/constants";

type BookSelectorProps = Readonly<{
  onBookSelect: (bookId: string) => void;
  selectedBookId?: string;
}>;

export function BookSelector({
  onBookSelect,
  selectedBookId,
}: BookSelectorProps) {
  const { data: booksData, isLoading } = useBooks({
    pageSize: DEFAULT_BOOKS_PAGE_SIZE,
  });

  const books = useMemo(() => {
    const items = booksData?.items ?? [];
    const uniqueBooks = new Map<string, (typeof items)[0]>();
    for (const book of items) {
      if (!uniqueBooks.has(book.id)) {
        uniqueBooks.set(book.id, book);
      }
    }
    return Array.from(uniqueBooks.values());
  }, [booksData?.items]);

  return (
    <div className="space-y-3">
      <label htmlFor="book-select" className="text-sm font-medium">
        Select a Book
      </label>
      <Select
        value={selectedBookId}
        onValueChange={onBookSelect}
        disabled={isLoading}
      >
        <SelectTrigger id="book-select">
          <SelectValue placeholder="Choose a book to view reviews..." />
        </SelectTrigger>
        <SelectContent>
          {books.map((book) => (
            <SelectItem key={book.id} value={book.id}>
              {book.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
