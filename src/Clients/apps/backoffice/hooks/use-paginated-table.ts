"use client";

import { useCallback, useState } from "react";

import type { SortingState } from "@tanstack/react-table";

const DEFAULT_PAGE_SIZE = 10;

type UsePaginatedTableOptions = {
  initialPageSize?: number;
};

export function usePaginatedTable(options?: UsePaginatedTableOptions) {
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize, setPageSize] = useState(
    options?.initialPageSize ?? DEFAULT_PAGE_SIZE,
  );
  const [sorting, setSorting] = useState<SortingState>([]);

  const handlePaginationChange = useCallback(
    (newPageIndex: number, newPageSize: number) => {
      setPageIndex(newPageIndex);
      setPageSize(newPageSize);
    },
    [],
  );

  const handleSortingChange = useCallback((newSorting: SortingState) => {
    setSorting(newSorting);
  }, []);

  const sortingQuery =
    sorting.length > 0 && sorting[0]
      ? {
          orderBy: sorting[0].id as string,
          isDescending: sorting[0].desc,
        }
      : {};

  return {
    pageIndex,
    pageSize,
    sorting,
    sortingQuery,
    /** 1-based page index for API calls */
    apiPageIndex: pageIndex + 1,
    handlePaginationChange,
    handleSortingChange,
  };
}
