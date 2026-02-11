"use client";

import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import type { Book, ListBooksQuery } from "@workspace/types/catalog/books";

import { FilterTable } from "@/components/filter-table";
import { usePaginatedTable } from "@/hooks/use-paginated-table";

import { columns } from "./columns";

type BooksTableProps = Readonly<{
  query: Omit<ListBooksQuery, "pageIndex" | "pageSize">;
  highlightedBookId?: string | null;
}>;

export function BooksTable({ query, highlightedBookId }: BooksTableProps) {
  const {
    pageIndex,
    pageSize,
    sortingQuery,
    apiPageIndex,
    handlePaginationChange,
    handleSortingChange,
  } = usePaginatedTable();

  const fullQuery: ListBooksQuery = {
    ...query,
    pageIndex: apiPageIndex,
    pageSize,
    ...sortingQuery,
  };

  const { data, isLoading } = useBooks(fullQuery);

  const books = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;

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
      onPaginationChange={handlePaginationChange}
      onSortingChange={handleSortingChange}
      highlightedId={highlightedBookId}
      getRowId={(row: Book) => row.id}
    />
  );
}
