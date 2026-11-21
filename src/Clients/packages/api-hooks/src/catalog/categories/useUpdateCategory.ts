"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import categoriesApiClient from "@workspace/api-client/catalog/categories";
import type {
  Category,
  UpdateCategoryRequest,
} from "@workspace/types/catalog/categories";

import { catalogKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useUpdateCategory(
  options?: UseMutationOptions<
    Category,
    Error,
    { request: UpdateCategoryRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: ({ request }) => categoriesApiClient.update(request),
      onSuccess: (data, variables) => {
        queryClient.setQueryData(
          catalogKeys.categories.detail(variables.request.id),
          data,
        );
        queryClient.invalidateQueries({
          queryKey: catalogKeys.categories.lists(),
        });
      },
    },
    options,
  );
}
