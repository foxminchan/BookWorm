"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import booksApiClient from "@workspace/api-client/catalog/books";
import type { UpdateBookRequest } from "@workspace/types/catalog/books";

import { catalogKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export type UpdateBookVariables = {
  request: UpdateBookRequest;
  ifMatch?: string;
};

export default function useUpdateBook(
  options?: UseMutationOptions<void, Error, UpdateBookVariables>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: ({ request, ifMatch }) =>
        booksApiClient.update(request, ifMatch ? { ifMatch } : undefined),
      onSuccess: (_, variables) => {
        queryClient.invalidateQueries({
          queryKey: catalogKeys.books.detail(variables.request.id),
        });
        queryClient.invalidateQueries({ queryKey: catalogKeys.books.lists() });
      },
    },
    options,
  );
}
