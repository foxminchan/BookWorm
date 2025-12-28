import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { basketApiClient } from "@workspace/api-client";
import type { CustomerBasket } from "@workspace/types/basket";
import { basketKeys } from "../keys";

export function useRemoveBasketItem(
  options?: UseMutationOptions<
    CustomerBasket,
    Error,
    { customerId: string; itemId: string }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, itemId }) =>
      basketApiClient.removeItem(customerId, itemId),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(basketKeys.detail(variables.customerId), data);
    },
    ...options,
  });
}
