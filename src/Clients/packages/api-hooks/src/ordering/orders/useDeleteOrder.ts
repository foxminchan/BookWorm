import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import ordersApiClient from "@workspace/api-client/ordering/orders";

import { orderingKeys } from "../../keys";

export default function useDeleteOrder(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => ordersApiClient.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: orderingKeys.orders.detail(id) });
      queryClient.invalidateQueries({ queryKey: orderingKeys.orders.lists() });
    },
    ...options,
  });
}
