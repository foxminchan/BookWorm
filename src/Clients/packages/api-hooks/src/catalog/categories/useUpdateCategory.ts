import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import categoriesApiClient from "@workspace/api-client/catalog/categories";
import type {
  Category,
  UpdateCategoryRequest,
} from "@workspace/types/catalog/categories";
import { catalogKeys } from "@/keys";

export default function useUpdateCategory(
  options?: UseMutationOptions<
    Category,
    Error,
    { request: UpdateCategoryRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
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
    ...options,
  });
}
