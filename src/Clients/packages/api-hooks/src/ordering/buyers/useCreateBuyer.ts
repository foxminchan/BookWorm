"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import buyersApiClient from "@workspace/api-client/ordering/buyers";
import type {
  Buyer,
  CreateBuyerRequest,
} from "@workspace/types/ordering/buyers";

import { orderingKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useCreateBuyer(
  options?: UseMutationOptions<Buyer, Error, CreateBuyerRequest>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (request) => buyersApiClient.create(request),
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: orderingKeys.buyers.lists(),
        });
      },
    },
    options,
  );
}
