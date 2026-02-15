"use client";

import useOrders from "@workspace/api-hooks/ordering/orders/useOrders";
import type {
  ListOrdersQuery,
  OrderStatus,
} from "@workspace/types/ordering/orders";

import { FilterTable } from "@/components/filter-table";
import { usePaginatedTable } from "@/hooks/usePaginatedTable";

import { columns } from "./columns";

type OrdersTableProps = Readonly<{
  statusFilter?: OrderStatus;
}>;

export function OrdersTable({ statusFilter }: OrdersTableProps) {
  const {
    pageIndex,
    pageSize,
    sortingQuery,
    apiPageIndex,
    handlePaginationChange,
    handleSortingChange,
  } = usePaginatedTable();

  const query: ListOrdersQuery = {
    pageIndex: apiPageIndex,
    pageSize,
    ...(statusFilter && { status: statusFilter }),
    ...sortingQuery,
  };

  const { data, isLoading } = useOrders(query);

  const orders = data?.items ?? [];
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
      onSortingChange={handleSortingChange}
    />
  );
}
