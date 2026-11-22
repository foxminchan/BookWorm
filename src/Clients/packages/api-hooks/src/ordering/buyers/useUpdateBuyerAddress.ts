"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import buyersApiClient from "@workspace/api-client/ordering/buyers";
import type {
  Buyer,
  UpdateAddressRequest,
} from "@workspace/types/ordering/buyers";

import { orderingKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useUpdateBuyerAddress(
  options?: UseMutationOptions<Buyer, Error, { request: UpdateAddressRequest }>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: ({ request }) => buyersApiClient.updateAddress(request),
      onSuccess: (data) => {
        queryClient.setQueryData(orderingKeys.buyers.current(), data);
        queryClient.invalidateQueries({
          queryKey: orderingKeys.buyers.lists(),
        });
      },
    },
    options,
  );
}
