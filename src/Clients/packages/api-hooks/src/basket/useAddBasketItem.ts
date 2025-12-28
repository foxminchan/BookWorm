import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { basketApiClient } from "@workspace/api-client";
import type { BasketItem, CustomerBasket } from "@workspace/types/basket";
import { basketKeys } from "../keys";

export function useAddBasketItem(
  options?: UseMutationOptions<
    CustomerBasket,
    Error,
    { customerId: string; item: BasketItem }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, item }) =>
      basketApiClient.addItem(customerId, item),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(basketKeys.detail(variables.customerId), data);
    },
    ...options,
  });
}
