"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { CustomerBasket } from "@workspace/types/basket";

import { useQueryFromOptions } from "../use-query-from-options";
import { basketQueries } from "./queries";

export default function useBasket(
  options?: Omit<UseQueryOptions<CustomerBasket>, "queryKey" | "queryFn">,
) {
  return useQueryFromOptions(basketQueries.detail(), options);
}
