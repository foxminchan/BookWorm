"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import ordersApiClient from "@workspace/api-client/ordering/orders";
import type { Order } from "@workspace/types/ordering/orders";

import { orderingKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useCompleteOrder(
  options?: UseMutationOptions<Order, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (id) => ordersApiClient.complete(id),
      onSuccess: (data, id) => {
        queryClient.setQueryData(orderingKeys.orders.detail(id), data);
        queryClient.invalidateQueries({
          queryKey: orderingKeys.orders.lists(),
        });
      },
    },
    options,
  );
}
