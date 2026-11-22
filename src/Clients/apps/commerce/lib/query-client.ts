import { getQueryClient as getSharedQueryClient } from "@workspace/api-hooks/query-client";

export function getQueryClient() {
  return getSharedQueryClient({
    refetchOnWindowFocus: false,
  });
}
