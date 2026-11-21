"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import authorsApiClient from "@workspace/api-client/catalog/authors";

import { catalogKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useDeleteAuthor(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (id) => authorsApiClient.delete(id),
      onSuccess: (_, id) => {
        queryClient.removeQueries({
          queryKey: catalogKeys.authors.detail(id),
        });
        queryClient.invalidateQueries({
          queryKey: catalogKeys.authors.lists(),
        });
      },
    },
    options,
  );
}
