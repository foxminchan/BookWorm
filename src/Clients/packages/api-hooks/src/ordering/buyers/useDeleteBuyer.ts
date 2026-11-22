"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import buyersApiClient from "@workspace/api-client/ordering/buyers";

import { orderingKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useDeleteBuyer(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (id) => buyersApiClient.delete(id),
      onSuccess: (_, id) => {
        queryClient.removeQueries({ queryKey: orderingKeys.buyers.detail(id) });
        queryClient.invalidateQueries({
          queryKey: orderingKeys.buyers.lists(),
        });
      },
    },
    options,
  );
}
