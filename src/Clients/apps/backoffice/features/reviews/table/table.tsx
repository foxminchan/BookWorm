"use client";

import useFeedbacks from "@workspace/api-hooks/rating/useFeedbacks";
import type { ListFeedbacksQuery } from "@workspace/types/rating";

import { FilterTable } from "@/components/filter-table";
import { usePaginatedTable } from "@/hooks/use-paginated-table";

import { reviewsColumns } from "./columns";

type ReviewsTableProps = Readonly<{
  bookId: string;
}>;

export function ReviewsTable({ bookId }: ReviewsTableProps) {
  const {
    pageIndex,
    pageSize,
    sortingQuery,
    apiPageIndex,
    handlePaginationChange,
    handleSortingChange,
  } = usePaginatedTable();

  const query: ListFeedbacksQuery = {
    bookId,
    pageIndex: apiPageIndex,
    pageSize,
    ...sortingQuery,
  };

  const { data, isLoading } = useFeedbacks(query);

  const reviews = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;

  return (
    <FilterTable
      columns={reviewsColumns}
      data={reviews}
      title="Book Reviews"
      description={`Total: ${totalCount} reviews`}
      totalCount={totalCount}
      pageIndex={pageIndex}
      pageSize={pageSize}
      isLoading={isLoading}
      onPaginationChange={handlePaginationChange}
      onSortingChange={handleSortingChange}
    />
  );
}
