import {
  type DefaultOptions,
  QueryClient,
  defaultShouldDehydrateQuery,
  isServer,
} from "@tanstack/react-query";

function makeQueryClient(
  queryOptions?: DefaultOptions["queries"],
): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 60 * 1000,
        ...queryOptions,
      },
      dehydrate: {
        shouldDehydrateQuery: (query) =>
          defaultShouldDehydrateQuery(query) ||
          query.state.status === "pending",
      },
    },
  });
}

let browserQueryClient: QueryClient | undefined = undefined;

/**
 * Gets or creates a QueryClient instance.
 *
 * Server: Always creates a new query client per request (prevents state sharing).
 * Browser: Reuses a single query client instance across renders.
 */
export function getQueryClient(
  queryOptions?: DefaultOptions["queries"],
): QueryClient {
  if (isServer) {
    return makeQueryClient(queryOptions);
  }

  browserQueryClient ??= makeQueryClient(queryOptions);

  return browserQueryClient;
}
