import { isServer } from "@tanstack/react-query";

import { getQueryClient as getSharedQueryClient } from "@workspace/api-hooks/query-client";

export function getQueryClient() {
  return getSharedQueryClient({
    refetchOnWindowFocus: false,
    retry: (failureCount, error) => {
      if (
        error &&
        typeof error === "object" &&
        "status" in error &&
        error.status === 404
      ) {
        return false;
      }
      if (isServer) return false;
      return failureCount < 2;
    },
  });
}
