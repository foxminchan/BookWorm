"use client";

import { useState } from "react";

import type { SortingState } from "@tanstack/react-table";

import useBuyers from "@workspace/api-hooks/ordering/buyers/useBuyers";
import type { ListBuyersQuery } from "@workspace/types/ordering/buyers";

import { FilterTable } from "@/components/filter-table";

import { columns } from "./columns";

const DEFAULT_PAGE_SIZE = 10;

export function CustomersTable() {
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [sorting, setSorting] = useState<SortingState>([]);

  const query: ListBuyersQuery = {
    pageIndex: pageIndex + 1,
    pageSize,
    ...(sorting.length > 0 && sorting[0]
      ? {
          orderBy: sorting[0].id as string,
          isDescending: sorting[0].desc,
        }
      : {}),
  };

  const { data, isLoading, error } = useBuyers(query);

  const customers = data?.items || [];
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
      data={customers}
      title="All Customers"
      description={`Total: ${totalCount} customers`}
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
