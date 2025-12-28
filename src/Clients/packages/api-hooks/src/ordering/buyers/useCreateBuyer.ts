import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { buyersApiClient } from "@workspace/api-client";
import type {
  Buyer,
  CreateBuyerRequest,
} from "@workspace/types/ordering/buyers";
import { orderingKeys } from "../../keys";

export function useCreateBuyer(
  options?: UseMutationOptions<Buyer, Error, CreateBuyerRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => buyersApiClient.createBuyer(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: orderingKeys.buyers.lists() });
    },
    ...options,
  });
}
