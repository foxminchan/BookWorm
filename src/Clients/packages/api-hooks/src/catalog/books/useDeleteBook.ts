import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { booksApiClient } from "@workspace/api-client";
import { catalogKeys } from "../../keys";

export function useDeleteBook(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => booksApiClient.deleteBook(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: catalogKeys.books.detail(id) });
      queryClient.invalidateQueries({ queryKey: catalogKeys.books.lists() });
    },
    ...options,
  });
}
