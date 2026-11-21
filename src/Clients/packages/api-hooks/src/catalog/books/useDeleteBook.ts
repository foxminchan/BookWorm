"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import booksApiClient from "@workspace/api-client/catalog/books";

import { deleteCachedETag } from "../../etag-cache";
import { catalogKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useDeleteBook(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (id) => booksApiClient.delete(id),
      onSuccess: (_, id) => {
        deleteCachedETag(queryClient, catalogKeys.books.detail(id));
        queryClient.removeQueries({ queryKey: catalogKeys.books.detail(id) });
        queryClient.invalidateQueries({ queryKey: catalogKeys.books.lists() });
      },
    },
    options,
  );
}
