import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import ordersApiClient from "@workspace/api-client/ordering/orders";

import { orderingKeys } from "../../keys";

export default function useCreateOrder(
  options?: UseMutationOptions<string, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (basketId) => ordersApiClient.create(basketId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: orderingKeys.orders.lists() });
    },
    ...options,
  });
}
