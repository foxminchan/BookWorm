import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import basketApiClient from "@workspace/api-client/basket/baskets";
import type {
  CustomerBasket,
  UpdateBasketRequest,
} from "@workspace/types/basket";
import { basketKeys } from "@/keys";

export default function useUpdateBasket(
  options?: UseMutationOptions<
    CustomerBasket,
    Error,
    { request: UpdateBasketRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ request }) => basketApiClient.update(request),
    onSuccess: (data) => {
      queryClient.setQueryData(basketKeys.detail(), data);
    },
    ...options,
  });
}
