import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import categoriesApiClient from "@workspace/api-client/catalog/categories";

import { catalogKeys } from "../../keys";

export default function useDeleteCategory(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => categoriesApiClient.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({
        queryKey: catalogKeys.categories.detail(id),
      });
      queryClient.invalidateQueries({
        queryKey: catalogKeys.categories.lists(),
      });
    },
    ...options,
  });
}
