"use client";

import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import useBuyers from "@workspace/api-hooks/ordering/buyers/useBuyers";
import useOrders from "@workspace/api-hooks/ordering/orders/useOrders";
import type { Book } from "@workspace/types/catalog/books";
import type { Buyer } from "@workspace/types/ordering/buyers";
import type { Order } from "@workspace/types/ordering/orders";

export function useDashboardStats() {
  const booksQuery = useBooks(undefined, { refetchInterval: 30000 });
  const ordersQuery = useOrders(undefined, { refetchInterval: 30000 });
  const buyersQuery = useBuyers(undefined, { refetchInterval: 30000 });

  return {
    books: booksQuery.data?.items || ([] as Book[]),
    orders: ordersQuery.data?.items || ([] as Order[]),
    customers: buyersQuery.data?.items || ([] as Buyer[]),
    isLoading:
      booksQuery.isLoading || ordersQuery.isLoading || buyersQuery.isLoading,
    error: booksQuery.error || ordersQuery.error || buyersQuery.error,
  };
}
