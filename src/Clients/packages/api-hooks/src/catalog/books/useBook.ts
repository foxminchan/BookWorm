import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { booksApiClient } from "@workspace/api-client";
import type { Book } from "@workspace/types/catalog/books";
import { catalogKeys } from "../../keys";

export function useBook(
  id: string,
  options?: Omit<UseQueryOptions<Book>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: catalogKeys.books.detail(id),
    queryFn: () => booksApiClient.getBook(id),
    ...options,
  });
}
