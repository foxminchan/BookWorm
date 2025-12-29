import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import booksApiClient from "@workspace/api-client/catalog/books";
import type { Book } from "@workspace/types/catalog/books";
import { catalogKeys } from "@/keys";

export default function useBook(
  id: string,
  options?: Omit<UseQueryOptions<Book>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: catalogKeys.books.detail(id),
    queryFn: () => booksApiClient.get(id),
    ...options,
  });
}
