import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import buyersApiClient from "@workspace/api-client/ordering/buyers";
import type {
  Buyer,
  UpdateAddressRequest,
} from "@workspace/types/ordering/buyers";
import { orderingKeys } from "@/keys";

export default function useUpdateBuyerAddress(
  options?: UseMutationOptions<Buyer, Error, { request: UpdateAddressRequest }>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ request }) => buyersApiClient.updateAddress(request),
    onSuccess: (data) => {
      queryClient.setQueryData(orderingKeys.buyers.current(), data);
      queryClient.invalidateQueries({ queryKey: orderingKeys.buyers.lists() });
    },
    ...options,
  });
}
