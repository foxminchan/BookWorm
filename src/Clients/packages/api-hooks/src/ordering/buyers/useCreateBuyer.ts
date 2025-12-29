import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import buyersApiClient from "@workspace/api-client/ordering/buyers";
import type {
  Buyer,
  CreateBuyerRequest,
} from "@workspace/types/ordering/buyers";
import { orderingKeys } from "@/keys";

export default function useCreateBuyer(
  options?: UseMutationOptions<Buyer, Error, CreateBuyerRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => buyersApiClient.create(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: orderingKeys.buyers.lists() });
    },
    ...options,
  });
}
