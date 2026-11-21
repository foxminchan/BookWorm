"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import categoriesApiClient from "@workspace/api-client/catalog/categories";
import type {
  Category,
  CreateCategoryRequest,
} from "@workspace/types/catalog/categories";

import { catalogKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useCreateCategory(
  options?: UseMutationOptions<Category, Error, CreateCategoryRequest>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (request) => categoriesApiClient.create(request),
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: catalogKeys.categories.lists(),
        });
      },
    },
    options,
  );
}
