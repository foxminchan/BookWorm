"use client";

import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import booksApiClient from "@workspace/api-client/catalog/books";

import { deleteCachedETag } from "../../etag-cache";
import { catalogKeys } from "../../keys";

export default function useDeleteBook(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => booksApiClient.delete(id),
    onSuccess: (_, id) => {
      deleteCachedETag(queryClient, catalogKeys.books.detail(id));
      queryClient.removeQueries({ queryKey: catalogKeys.books.detail(id) });
      queryClient.invalidateQueries({ queryKey: catalogKeys.books.lists() });
    },
    ...options,
  });
}
