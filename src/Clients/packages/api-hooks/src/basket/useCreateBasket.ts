"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import basketApiClient from "@workspace/api-client/basket/baskets";
import type {
  CreateBasketRequest,
  CustomerBasket,
} from "@workspace/types/basket";

import { basketKeys } from "../keys";
import { useMutationFromOptions } from "../use-mutation-from-options";

export default function useCreateBasket(
  options?: UseMutationOptions<CustomerBasket, Error, CreateBasketRequest>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (request) => basketApiClient.create(request),
      onSuccess: (data) => {
        queryClient.setQueryData(basketKeys.detail(), data);
      },
    },
    options,
  );
}
