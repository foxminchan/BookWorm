import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import buyersApiClient from "@workspace/api-client/ordering/buyers";
import { orderingKeys } from "../../keys";

export default function useDeleteBuyer(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => buyersApiClient.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: orderingKeys.buyers.detail(id) });
      queryClient.invalidateQueries({ queryKey: orderingKeys.buyers.lists() });
    },
    ...options,
  });
}
