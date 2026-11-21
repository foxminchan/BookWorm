"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { OrderEventHistory } from "@workspace/types/ordering/orders";

import { useQueryFromOptions } from "../../use-query-from-options";
import { orderQueries } from "../queries";

export default function useOrderHistory(
  id: string,
  options?: Omit<UseQueryOptions<OrderEventHistory[]>, "queryKey" | "queryFn">,
) {
  return useQueryFromOptions(orderQueries.history(id), options);
}
