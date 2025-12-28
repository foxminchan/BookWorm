import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { basketApiClient } from "@workspace/api-client";
import { basketKeys } from "../keys";

export function useDeleteBasket(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (customerId) => basketApiClient.deleteBasket(customerId),
    onSuccess: (_, customerId) => {
      queryClient.removeQueries({ queryKey: basketKeys.detail(customerId) });
    },
    ...options,
  });
}
