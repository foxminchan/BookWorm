"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { Publisher } from "@workspace/types/catalog/publishers";

import { useQueryFromOptions } from "../../use-query-from-options";
import { publisherQueries } from "../queries";

export default function usePublishers(
  options?: Omit<UseQueryOptions<Publisher[]>, "queryKey" | "queryFn">,
) {
  return useQueryFromOptions(publisherQueries.list(), options);
}
