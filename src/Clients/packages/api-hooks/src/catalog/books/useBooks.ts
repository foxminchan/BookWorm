"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { Book, ListBooksQuery } from "@workspace/types/catalog/books";
import type { PagedResult } from "@workspace/types/shared";

import { useQueryFromOptions } from "../../use-query-from-options";
import { bookQueries } from "../queries";

export default function useBooks(
  query?: ListBooksQuery,
  options?: Omit<UseQueryOptions<PagedResult<Book>>, "queryKey" | "queryFn">,
) {
  return useQueryFromOptions(bookQueries.list(query), options);
}
