"use client";

import { useState } from "react";

import type { SortingState } from "@tanstack/react-table";

import useOrders from "@workspace/api-hooks/ordering/orders/useOrders";
import type { ListOrdersQuery } from "@workspace/types/ordering/orders";

import { FilterTable } from "@/components/filter-table";

import { columns } from "./columns";

type OrdersTableProps = {
  statusFilter?: string;
};

const DEFAULT_PAGE_SIZE = 10;

export function OrdersTable({ statusFilter }: OrdersTableProps) {
  const [pageIndex, setPageIndex] = useState(0);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [sorting, setSorting] = useState<SortingState>([]);

  const query: ListOrdersQuery = {
    pageIndex: pageIndex + 1,
    pageSize,
    ...(statusFilter && { status: statusFilter as any }),
    ...(sorting.length > 0 && sorting[0]
      ? {
          orderBy: sorting[0].id as any,
          isDescending: sorting[0].desc,
        }
      : {}),
  };

  const { data, isLoading, error } = useOrders(query);

  const orders = data?.items || [];
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
      data={orders}
      title="All Orders"
      description={`Total: ${totalCount} orders`}
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
