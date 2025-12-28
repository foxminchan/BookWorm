import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { basketApiClient } from "@workspace/api-client";
import type {
  CustomerBasket,
  UpdateBasketRequest,
} from "@workspace/types/basket";
import { basketKeys } from "../keys";

export function useUpdateBasket(
  options?: UseMutationOptions<
    CustomerBasket,
    Error,
    { customerId: string; request: UpdateBasketRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, request }) =>
      basketApiClient.updateBasket(customerId, request),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(basketKeys.detail(variables.customerId), data);
    },
    ...options,
  });
}
