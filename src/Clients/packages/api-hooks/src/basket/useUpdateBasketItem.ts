import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { basketApiClient } from "@workspace/api-client";
import type { BasketItem, CustomerBasket } from "@workspace/types/basket";
import { basketKeys } from "../keys";

export function useUpdateBasketItem(
  options?: UseMutationOptions<
    CustomerBasket,
    Error,
    { customerId: string; itemId: string; item: Partial<BasketItem> }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, itemId, item }) =>
      basketApiClient.updateItem(customerId, itemId, item),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(basketKeys.detail(variables.customerId), data);
    },
    ...options,
  });
}
