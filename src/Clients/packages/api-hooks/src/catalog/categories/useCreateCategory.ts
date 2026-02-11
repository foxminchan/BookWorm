import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import categoriesApiClient from "@workspace/api-client/catalog/categories";
import type {
  Category,
  CreateCategoryRequest,
} from "@workspace/types/catalog/categories";

import { catalogKeys } from "../../keys";

export default function useCreateCategory(
  options?: UseMutationOptions<Category, Error, CreateCategoryRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => categoriesApiClient.create(request),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: catalogKeys.categories.lists(),
      });
    },
    ...options,
  });
}
