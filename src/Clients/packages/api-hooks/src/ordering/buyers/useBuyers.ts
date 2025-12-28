import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { buyersApiClient } from "@workspace/api-client";
import type { Buyer, ListBuyersQuery } from "@workspace/types/ordering/buyers";
import type { PagedResult } from "@workspace/types/shared";
import { orderingKeys } from "../../keys";

export function useBuyers(
  query?: ListBuyersQuery,
  options?: Omit<UseQueryOptions<PagedResult<Buyer>>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: orderingKeys.buyers.list(query),
    queryFn: () => buyersApiClient.listBuyers(query),
    ...options,
  });
}
