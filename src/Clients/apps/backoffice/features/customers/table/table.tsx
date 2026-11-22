"use client";

import useBuyers from "@workspace/api-hooks/ordering/buyers/useBuyers";
import type { Buyer, ListBuyersQuery } from "@workspace/types/ordering/buyers";

import { FilterTable } from "@/components/filter-table";
import { usePaginatedTable } from "@/hooks/usePaginatedTable";

import { columns } from "./columns";

const EMPTY_CUSTOMERS: Buyer[] = [];

export function CustomersTable() {
  const {
    pageIndex,
    pageSize,
    sorting,
    sortingQuery,
    apiPageIndex,
    handlePaginationChange,
    handleSortingChange,
  } = usePaginatedTable();

  const query: ListBuyersQuery = {
    pageIndex: apiPageIndex,
    pageSize,
    ...sortingQuery,
  };

  const { data, isLoading } = useBuyers(query);

  const customers = data?.items ?? EMPTY_CUSTOMERS;
  const totalCount = data?.totalCount ?? 0;

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
      onPaginationChange={handlePaginationChange}
      sorting={sorting}
      onSortingChange={handleSortingChange}
    />
  );
}
