import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { buyersApiClient } from "@workspace/api-client";
import type {
  Buyer,
  UpdateAddressRequest,
} from "@workspace/types/ordering/buyers";
import { orderingKeys } from "../../keys";

export function useUpdateBuyerAddress(
  options?: UseMutationOptions<
    Buyer,
    Error,
    { id: string; request: UpdateAddressRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }) => buyersApiClient.updateAddress(id, request),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(orderingKeys.buyers.detail(variables.id), data);
      queryClient.invalidateQueries({ queryKey: orderingKeys.buyers.lists() });
    },
    ...options,
  });
}
