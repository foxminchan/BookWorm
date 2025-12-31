import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import booksApiClient from "@workspace/api-client/catalog/books";
import type { Book, UpdateBookRequest } from "@workspace/types/catalog/books";
import { catalogKeys } from "../../keys";

export default function useUpdateBook(
  options?: UseMutationOptions<Book, Error, { request: UpdateBookRequest }>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ request }) => booksApiClient.update(request),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(
        catalogKeys.books.detail(variables.request.id),
        data,
      );
      queryClient.invalidateQueries({ queryKey: catalogKeys.books.lists() });
    },
    ...options,
  });
}
