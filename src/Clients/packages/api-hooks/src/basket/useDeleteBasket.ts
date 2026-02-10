import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import basketApiClient from "@workspace/api-client/basket/baskets";

import { basketKeys } from "../keys";

export default function useDeleteBasket(
  options?: UseMutationOptions<void, Error, void>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => basketApiClient.delete(),
    onSuccess: () => {
      queryClient.setQueryData(basketKeys.detail(), null);
    },
    ...options,
  });
}
