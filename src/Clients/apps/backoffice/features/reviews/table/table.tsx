"use client";

import { useEffect } from "react";

import useFeedbacks from "@workspace/api-hooks/rating/useFeedbacks";
import type { Feedback, ListFeedbacksQuery } from "@workspace/types/rating";
import { TooltipProvider } from "@workspace/ui/components/tooltip";

import { FilterTable } from "@/components/filter-table";
import { usePaginatedTable } from "@/hooks/usePaginatedTable";

import { reviewsColumns } from "./columns";

const EMPTY_REVIEWS: Feedback[] = [];

type ReviewsTableProps = Readonly<{
  bookId: string;
}>;

export function ReviewsTable({ bookId }: ReviewsTableProps) {
  const {
    pageIndex,
    pageSize,
    sorting,
    sortingQuery,
    apiPageIndex,
    handlePaginationChange,
    handleSortingChange,
    resetPage,
  } = usePaginatedTable();

  useEffect(() => {
    resetPage();
  }, [bookId, resetPage]);

  const query: ListFeedbacksQuery = {
    bookId,
    pageIndex: apiPageIndex,
    pageSize,
    ...sortingQuery,
  };

  const { data, isLoading } = useFeedbacks(query);

  const reviews = data?.items ?? EMPTY_REVIEWS;
  const totalCount = data?.totalCount ?? 0;

  return (
    <TooltipProvider>
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
        sorting={sorting}
        onSortingChange={handleSortingChange}
      />
    </TooltipProvider>
  );
}
