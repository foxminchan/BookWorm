"use client";

import { useState } from "react";

import type { SortingState } from "@tanstack/react-table";

import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import type { Book, ListBooksQuery } from "@workspace/types/catalog/books";

import { FilterTable } from "@/components/filter-table";

import { columns } from "./columns";

type BooksTableProps = {
  query: Omit<ListBooksQuery, "pageIndex" | "pageSize">;
  highlightedBookId?: string | null;
};

const DEFAULT_PAGE_SIZE = 10;

export function BooksTable({ query, highlightedBookId }: BooksTableProps) {
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [sorting, setSorting] = useState<SortingState>([]);

  const fullQuery: ListBooksQuery = {
    ...query,
    pageIndex: pageIndex + 1,
    pageSize,
    ...(sorting.length > 0 && sorting[0]
      ? {
          orderBy: sorting[0].id as string,
          isDescending: sorting[0].desc,
        }
      : {}),
  };

  const { data, isLoading, error } = useBooks(fullQuery);

  const books = data?.items || [];
  const totalCount = data?.totalCount || 0;

  const handlePaginationChange = (
    newPageIndex: number,
    newPageSize: number,
  ) => {
    setPageIndex(newPageIndex);
    setPageSize(newPageSize);
  };

  const handleSortingChange = (newSorting: SortingState) => {
    setSorting(newSorting);
  };

  return (
    <FilterTable
      columns={columns}
      data={books}
      title="All Books"
      description={`Total: ${totalCount} books`}
      totalCount={totalCount}
      pageIndex={pageIndex}
      pageSize={pageSize}
      isLoading={isLoading}
      error={error}
      onPaginationChange={handlePaginationChange}
      onSortingChange={handleSortingChange}
      highlightedId={highlightedBookId}
      getRowId={(row: Book) => row.id}
    />
  );
}
