"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import categoriesApiClient from "@workspace/api-client/catalog/categories";

import { catalogKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useDeleteCategory(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (id) => categoriesApiClient.delete(id),
      onSuccess: (_, id) => {
        queryClient.removeQueries({
          queryKey: catalogKeys.categories.detail(id),
        });
        queryClient.invalidateQueries({
          queryKey: catalogKeys.categories.lists(),
        });
      },
    },
    options,
  );
}
