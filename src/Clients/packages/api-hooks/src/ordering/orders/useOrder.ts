"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { OrderDetail } from "@workspace/types/ordering/orders";

import { useQueryFromOptions } from "../../use-query-from-options";
import { orderQueries } from "../queries";

export default function useOrder(
  id: string,
  options?: Omit<UseQueryOptions<OrderDetail>, "queryKey" | "queryFn">,
) {
  return useQueryFromOptions(orderQueries.detail(id), options);
}
