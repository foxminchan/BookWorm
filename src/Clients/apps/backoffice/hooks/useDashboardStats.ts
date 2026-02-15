"use client";

import { useMemo } from "react";

import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import useBuyers from "@workspace/api-hooks/ordering/buyers/useBuyers";
import useOrders from "@workspace/api-hooks/ordering/orders/useOrders";

const REFETCH_INTERVAL = 30_000;

export function useDashboardStats() {
  const booksQuery = useBooks(undefined, { refetchInterval: REFETCH_INTERVAL });
  const ordersQuery = useOrders(undefined, {
    refetchInterval: REFETCH_INTERVAL,
  });
  const buyersQuery = useBuyers(undefined, {
    refetchInterval: REFETCH_INTERVAL,
  });

  return useMemo(
    () => ({
      books: booksQuery.data?.items ?? [],
      orders: ordersQuery.data?.items ?? [],
      customers: buyersQuery.data?.items ?? [],
      isLoading:
        booksQuery.isLoading || ordersQuery.isLoading || buyersQuery.isLoading,
      error: booksQuery.error ?? ordersQuery.error ?? buyersQuery.error,
    }),
    [
      booksQuery.data?.items,
      booksQuery.isLoading,
      booksQuery.error,
      ordersQuery.data?.items,
      ordersQuery.isLoading,
      ordersQuery.error,
      buyersQuery.data?.items,
      buyersQuery.isLoading,
      buyersQuery.error,
    ],
  );
}
