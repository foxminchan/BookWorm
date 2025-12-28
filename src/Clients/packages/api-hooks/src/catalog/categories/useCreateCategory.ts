import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { categoriesApiClient } from "@workspace/api-client";
import type {
  Category,
  CreateCategoryRequest,
} from "@workspace/types/catalog/categories";
import { catalogKeys } from "../../keys";

export function useCreateCategory(
  options?: UseMutationOptions<Category, Error, CreateCategoryRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => categoriesApiClient.createCategory(request),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: catalogKeys.categories.lists(),
      });
    },
    ...options,
  });
}
