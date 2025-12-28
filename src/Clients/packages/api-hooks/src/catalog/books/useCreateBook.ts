import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { booksApiClient } from "@workspace/api-client";
import type { Book, CreateBookRequest } from "@workspace/types/catalog/books";
import { catalogKeys } from "../../keys";

export function useCreateBook(
  options?: UseMutationOptions<Book, Error, CreateBookRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => booksApiClient.createBook(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: catalogKeys.books.lists() });
    },
    ...options,
  });
}
