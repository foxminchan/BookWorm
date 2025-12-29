import { orderingKeys } from "@/keys";
import {
  UseMutationOptions,
  useQueryClient,
  useMutation,
} from "@tanstack/react-query";
import ordersApiClient from "@workspace/api-client/ordering/orders";

export default function useDeleteOrder(
  id: string,
  options?: Omit<UseMutationOptions<void, unknown, void>, "mutationFn">,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => ordersApiClient.delete(id),
    onSuccess: () => {
      queryClient.removeQueries({ queryKey: orderingKeys.orders.detail(id) });
      queryClient.invalidateQueries({ queryKey: orderingKeys.orders.lists() });
    },
    ...options,
  });
}
