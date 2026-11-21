import {
  type DefaultOptions,
  QueryClient,
  defaultShouldDehydrateQuery,
} from "@tanstack/react-query";

const isServerEnvironment = () => typeof window === "undefined";

const MAX_CLIENT_QUERY_RETRIES = 2;

function getErrorStatus(error: unknown): number | undefined {
  if (!error || typeof error !== "object") {
    return undefined;
  }

  if ("status" in error && typeof error.status === "number") {
    return error.status;
  }

  if (
    "response" in error &&
    error.response &&
    typeof error.response === "object" &&
    "status" in error.response &&
    typeof error.response.status === "number"
  ) {
    return error.response.status;
  }

  return undefined;
}

function shouldRetryQuery(failureCount: number, error: unknown): boolean {
  if (isServerEnvironment()) {
    return false;
  }

  const status = getErrorStatus(error);
  if (
    status !== undefined &&
    status >= 400 &&
    status < 500 &&
    status !== 408 &&
    status !== 429
  ) {
    return false;
  }

  return failureCount < MAX_CLIENT_QUERY_RETRIES;
}

function makeQueryClient(
  queryOptions?: DefaultOptions["queries"],
): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 60 * 1000,
        retry: shouldRetryQuery,
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
  if (isServerEnvironment()) {
    return makeQueryClient(queryOptions);
  }

  browserQueryClient ??= makeQueryClient(queryOptions);

  return browserQueryClient;
}
