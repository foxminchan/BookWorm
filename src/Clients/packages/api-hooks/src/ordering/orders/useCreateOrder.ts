"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import ordersApiClient from "@workspace/api-client/ordering/orders";

import { orderingKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useCreateOrder(
  options?: UseMutationOptions<string, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (basketId) => ordersApiClient.create(basketId),
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: orderingKeys.orders.lists(),
        });
      },
    },
    options,
  );
}
