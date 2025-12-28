import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { ordersApiClient } from "@workspace/api-client";
import type { Order } from "@workspace/types/ordering/orders";
import { orderingKeys } from "../../keys";

export function useCreateOrder(
  options?: UseMutationOptions<Order, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (basketId) => ordersApiClient.createOrder(basketId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: orderingKeys.orders.lists() });
    },
    ...options,
  });
}
