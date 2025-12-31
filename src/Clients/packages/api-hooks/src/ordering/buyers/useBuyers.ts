import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import buyersApiClient from "@workspace/api-client/ordering/buyers";
import type { Buyer, ListBuyersQuery } from "@workspace/types/ordering/buyers";
import type { PagedResult } from "@workspace/types/shared";
import { orderingKeys } from "../../keys";

export default function useBuyers(
  query?: ListBuyersQuery,
  options?: Omit<UseQueryOptions<PagedResult<Buyer>>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: orderingKeys.buyers.list(query),
    queryFn: () => buyersApiClient.list(query),
    ...options,
  });
}
