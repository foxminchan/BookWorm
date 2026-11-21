"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { Author } from "@workspace/types/catalog/authors";

import { useQueryFromOptions } from "../../use-query-from-options";
import { authorQueries } from "../queries";

export default function useAuthors(
  options?: Omit<UseQueryOptions<Author[]>, "queryKey" | "queryFn">,
) {
  return useQueryFromOptions(authorQueries.list(), options);
}
