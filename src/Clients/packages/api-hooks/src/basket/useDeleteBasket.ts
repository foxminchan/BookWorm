"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import basketApiClient from "@workspace/api-client/basket/baskets";

import { basketKeys } from "../keys";
import { useMutationFromOptions } from "../use-mutation-from-options";

export default function useDeleteBasket(
  options?: UseMutationOptions<void, Error, void>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: () => basketApiClient.delete(),
      onSuccess: () => {
        queryClient.setQueryData(basketKeys.detail(), null);
      },
    },
    options,
  );
}
