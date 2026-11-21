"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { Buyer, ListBuyersQuery } from "@workspace/types/ordering/buyers";
import type { PagedResult } from "@workspace/types/shared";

import { useQueryFromOptions } from "../../use-query-from-options";
import { buyerQueries } from "../queries";

export default function useBuyers(
  query?: ListBuyersQuery,
  options?: Omit<UseQueryOptions<PagedResult<Buyer>>, "queryKey" | "queryFn">,
) {
  return useQueryFromOptions(buyerQueries.list(query), options);
}
