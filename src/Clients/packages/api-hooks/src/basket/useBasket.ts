import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import basketApiClient from "@workspace/api-client/basket/baskets";
import type { CustomerBasket } from "@workspace/types/basket";
import { basketKeys } from "@/keys";

export default function useBasket(
  options?: Omit<UseQueryOptions<CustomerBasket>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: basketKeys.detail(),
    queryFn: () => basketApiClient.get(),
    ...options,
  });
}
