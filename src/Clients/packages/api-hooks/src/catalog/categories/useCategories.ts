"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { Category } from "@workspace/types/catalog/categories";

import { useQueryFromOptions } from "../../use-query-from-options";
import { categoryQueries } from "../queries";

export default function useCategories(
  options?: Omit<UseQueryOptions<Category[]>, "queryKey" | "queryFn">,
) {
  return useQueryFromOptions(categoryQueries.list(), options);
}
