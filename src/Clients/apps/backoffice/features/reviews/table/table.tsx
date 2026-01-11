"use client";

import { useState } from "react";

import type { SortingState } from "@tanstack/react-table";

import useFeedbacks from "@workspace/api-hooks/rating/useFeedbacks";
import type { ListFeedbacksQuery } from "@workspace/types/rating";

import { FilterTable } from "@/components/filter-table";

import { reviewsColumns } from "./columns";

type ReviewsTableProps = {
  bookId: string;
};

const DEFAULT_PAGE_SIZE = 10;

export function ReviewsTable({ bookId }: ReviewsTableProps) {
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [sorting, setSorting] = useState<SortingState>([]);

  const query: ListFeedbacksQuery = {
    bookId,
    pageIndex: pageIndex + 1,
    pageSize,
    ...(sorting.length > 0 && sorting[0]
      ? {
          orderBy: sorting[0].id as string,
          isDescending: sorting[0].desc,
        }
      : {}),
  };

  const { data, isLoading, error } = useFeedbacks(query);

  const reviews = data?.items || [];
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
      columns={reviewsColumns}
      data={reviews}
      title="Book Reviews"
      description={`Total: ${totalCount} reviews`}
      totalCount={totalCount}
      pageIndex={pageIndex}
      pageSize={pageSize}
      isLoading={isLoading}
      error={error}
      onPaginationChange={handlePaginationChange}
      onSortingChange={handleSortingChange}
    />
  );
}
