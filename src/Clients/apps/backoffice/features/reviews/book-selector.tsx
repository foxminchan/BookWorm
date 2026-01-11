"use client";

import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";

import { DEFAULT_BOOKS_PAGE_SIZE } from "@/lib/constants";

type BookSelectorProps = {
  onBookSelect: (bookId: string) => void;
  selectedBookId?: string;
};

export function BookSelector({
  onBookSelect,
  selectedBookId,
}: BookSelectorProps) {
  const { data: booksData, isLoading } = useBooks({
    pageSize: DEFAULT_BOOKS_PAGE_SIZE,
  });

  const items = booksData?.items || [];
  const uniqueBooks = items.reduce((acc, book) => {
    if (!acc.has(book.id)) {
      acc.set(book.id, book);
    }
    return acc;
  }, new Map<string, (typeof items)[0]>());
  const books = Array.from(uniqueBooks.values());

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
