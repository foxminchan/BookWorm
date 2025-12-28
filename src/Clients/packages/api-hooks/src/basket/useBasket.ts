import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { basketApiClient } from "@workspace/api-client";
import type { CustomerBasket } from "@workspace/types/basket";
import { basketKeys } from "../keys";

export function useBasket(
  customerId: string,
  options?: Omit<UseQueryOptions<CustomerBasket>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: basketKeys.detail(customerId),
    queryFn: () => basketApiClient.getBasket(customerId),
    ...options,
  });
}
