"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import basketApiClient from "@workspace/api-client/basket/baskets";
import type {
  CustomerBasket,
  UpdateBasketRequest,
} from "@workspace/types/basket";

import { basketKeys } from "../keys";
import { useMutationFromOptions } from "../use-mutation-from-options";

export default function useUpdateBasket(
  options?: UseMutationOptions<
    CustomerBasket,
    Error,
    { request: UpdateBasketRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: ({ request }) => basketApiClient.update(request),
      onSuccess: (data) => {
        queryClient.setQueryData(basketKeys.detail(), data);
      },
    },
    options,
  );
}
