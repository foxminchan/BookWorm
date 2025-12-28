import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { booksApiClient } from "@workspace/api-client";
import type { Book, UpdateBookRequest } from "@workspace/types/catalog/books";
import { catalogKeys } from "../../keys";

export function useUpdateBook(
  options?: UseMutationOptions<
    Book,
    Error,
    { id: string; request: UpdateBookRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }) => booksApiClient.updateBook(id, request),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(catalogKeys.books.detail(variables.id), data);
      queryClient.invalidateQueries({ queryKey: catalogKeys.books.lists() });
    },
    ...options,
  });
}
