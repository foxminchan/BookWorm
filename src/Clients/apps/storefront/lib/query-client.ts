import {
  QueryClient,
  defaultShouldDehydrateQuery,
  isServer,
} from "@tanstack/react-query";

/**
 * Creates a new QueryClient instance with optimized settings for SSR.
 *
 * Key configurations:
 * - staleTime: 60s to avoid immediate refetch on client after SSR hydration
 * - Dehydrates pending queries for server streaming support
 * - Does not redact errors in Next.js (Next.js handles this automatically)
 */
function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        // With SSR, we usually want to set some default staleTime
        // above 0 to avoid refetching immediately on the client
        staleTime: 60 * 1000,
        refetchOnWindowFocus: false,
        retry: (failureCount, error) => {
          // Don't retry on 404s or during SSR
          if (
            error &&
            typeof error === "object" &&
            "status" in error &&
            error.status === 404
          )
            return false;
          if (isServer) return false;
          return failureCount < 2;
        },
      },
      dehydrate: {
        // Include pending queries in dehydration for server streaming
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
 * Server: Always creates a new query client per request (prevents state sharing)
 * Browser: Creates a single query client and reuses it (important for React Suspense)
 *
 * @returns QueryClient instance
 */
export function getQueryClient() {
  if (isServer) {
    // Server: always make a new query client
    return makeQueryClient();
  }

  // Browser: make a new query client if we don't already have one
  // This is very important, so we don't re-make a new client if React
  // suspends during the initial render. This may not be needed if we
  // have a suspense boundary BELOW the creation of the query client
  browserQueryClient ??= makeQueryClient();

  return browserQueryClient;
}
