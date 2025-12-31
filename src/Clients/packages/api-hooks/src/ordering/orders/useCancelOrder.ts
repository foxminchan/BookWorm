import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import ordersApiClient from "@workspace/api-client/ordering/orders";
import type { Order } from "@workspace/types/ordering/orders";
import { orderingKeys } from "../../keys";

export default function useCancelOrder(
  options?: UseMutationOptions<Order, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => ordersApiClient.cancel(id),
    onSuccess: (data, id) => {
      queryClient.setQueryData(orderingKeys.orders.detail(id), data);
      queryClient.invalidateQueries({ queryKey: orderingKeys.orders.lists() });
    },
    ...options,
  });
}
