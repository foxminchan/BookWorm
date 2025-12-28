import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { basketApiClient } from "@workspace/api-client";
import type {
  CreateBasketRequest,
  CustomerBasket,
} from "@workspace/types/basket";
import { basketKeys } from "../keys";

export function useCreateBasket(
  customerId: string,
  options?: UseMutationOptions<CustomerBasket, Error, CreateBasketRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => basketApiClient.createBasket(request),
    onSuccess: (data) => {
      queryClient.setQueryData(basketKeys.detail(customerId), data);
    },
    ...options,
  });
}
