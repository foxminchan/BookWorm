import { type UseQueryOptions, useQuery } from "@tanstack/react-query";

import booksApiClient from "@workspace/api-client/catalog/books";
import type { Book, ListBooksQuery } from "@workspace/types/catalog/books";
import type { PagedResult } from "@workspace/types/shared";

import { catalogKeys } from "../../keys";

export default function useBooks(
  query?: ListBooksQuery,
  options?: Omit<UseQueryOptions<PagedResult<Book>>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: catalogKeys.books.list(query),
    queryFn: () => booksApiClient.list(query),
    ...options,
  });
}
