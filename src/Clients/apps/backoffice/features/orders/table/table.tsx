"use client";

import { useEffect } from "react";

import useOrders from "@workspace/api-hooks/ordering/orders/useOrders";
import type {
  ListOrdersQuery,
  Order,
  OrderStatus,
} from "@workspace/types/ordering/orders";

import { FilterTable } from "@/components/filter-table";
import { usePaginatedTable } from "@/hooks/usePaginatedTable";

import { columns } from "./columns";

const EMPTY_ORDERS: Order[] = [];

type OrdersTableProps = Readonly<{
  statusFilter?: OrderStatus;
}>;

export function OrdersTable({ statusFilter }: OrdersTableProps) {
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
  }, [statusFilter, resetPage]);

  const query: ListOrdersQuery = {
    pageIndex: apiPageIndex,
    pageSize,
    ...(statusFilter && { status: statusFilter }),
    ...sortingQuery,
  };

  const { data, isLoading } = useOrders(query);

  const orders = data?.items ?? EMPTY_ORDERS;
  const totalCount = data?.totalCount ?? 0;

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
      onPaginationChange={handlePaginationChange}
      sorting={sorting}
      onSortingChange={handleSortingChange}
    />
  );
}
