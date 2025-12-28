import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { buyersApiClient } from "@workspace/api-client";
import type { Buyer } from "@workspace/types/ordering/buyers";
import { orderingKeys } from "../../keys";

export function useBuyer(
  id: string,
  options?: Omit<UseQueryOptions<Buyer>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: orderingKeys.buyers.detail(id),
    queryFn: () => buyersApiClient.getBuyer(id),
    ...options,
  });
}
