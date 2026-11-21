"use client";

import {
  type QueryKey,
  type UseQueryOptions,
  type UseQueryResult,
  useQuery,
} from "@tanstack/react-query";

/**
 * Executes `useQuery` from a `queryOptions` factory while preserving the
 * `Omit<UseQueryOptions<TData>, "queryKey" | "queryFn">` override contract.
 */
export function useQueryFromOptions<TData, TQueryKey extends QueryKey>(
  factoryOptions: UseQueryOptions<TData, Error, TData, TQueryKey>,
  overrides?: Omit<UseQueryOptions<TData>, "queryKey" | "queryFn">,
): UseQueryResult<TData, Error> {
  return useQuery({
    ...(factoryOptions as unknown as UseQueryOptions<
      TData,
      Error,
      TData,
      QueryKey
    >),
    ...overrides,
  });
}
