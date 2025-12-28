import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { categoriesApiClient } from "@workspace/api-client";
import type {
  Category,
  UpdateCategoryRequest,
} from "@workspace/types/catalog/categories";
import { catalogKeys } from "../../keys";

export function useUpdateCategory(
  options?: UseMutationOptions<
    Category,
    Error,
    { id: string; request: UpdateCategoryRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }) =>
      categoriesApiClient.updateCategory(id, request),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(
        catalogKeys.categories.detail(variables.id),
        data,
      );
      queryClient.invalidateQueries({
        queryKey: catalogKeys.categories.lists(),
      });
    },
    ...options,
  });
}
