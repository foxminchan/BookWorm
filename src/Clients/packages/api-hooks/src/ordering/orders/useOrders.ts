"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { ListOrdersQuery, Order } from "@workspace/types/ordering/orders";
import type { PagedResult } from "@workspace/types/shared";

import { useQueryFromOptions } from "../../use-query-from-options";
import { orderQueries } from "../queries";

export default function useOrders(
  query?: ListOrdersQuery,
  options?: Omit<UseQueryOptions<PagedResult<Order>>, "queryKey" | "queryFn">,
) {
  return useQueryFromOptions(orderQueries.list(query), options);
}
