import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { authorsApiClient } from "@workspace/api-client";
import { catalogKeys } from "../../keys";

export function useDeleteAuthor(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => authorsApiClient.deleteAuthor(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: catalogKeys.authors.detail(id) });
      queryClient.invalidateQueries({ queryKey: catalogKeys.authors.lists() });
    },
    ...options,
  });
}
