"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { Buyer } from "@workspace/types/ordering/buyers";

import { useQueryFromOptions } from "../../use-query-from-options";
import { buyerQueries } from "../queries";

export default function useBuyer(
  options?: Omit<UseQueryOptions<Buyer>, "queryKey" | "queryFn">,
) {
  return useQueryFromOptions(buyerQueries.current(), options);
}
