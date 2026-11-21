import {
  type QueryClient,
  type QueryKey,
  hashKey,
} from "@tanstack/react-query";

import {
  type ConditionalResponse,
  formatETag,
} from "@workspace/api-client/client";

const etagStores = new WeakMap<QueryClient, Map<string, string>>();

function getStore(queryClient: QueryClient): Map<string, string> {
  let store = etagStores.get(queryClient);
  if (!store) {
    store = new Map<string, string>();
    etagStores.set(queryClient, store);
  }
  return store;
}

export function getCachedETag(
  queryClient: QueryClient,
  queryKey: QueryKey,
): string | undefined {
  return etagStores.get(queryClient)?.get(hashKey(queryKey));
}

export function setCachedETag(
  queryClient: QueryClient,
  queryKey: QueryKey,
  etag: string | undefined,
): void {
  const key = hashKey(queryKey);
  if (etag) {
    getStore(queryClient).set(key, etag);
  } else {
    etagStores.get(queryClient)?.delete(key);
  }
}

export function deleteCachedETag(
  queryClient: QueryClient,
  queryKey: QueryKey,
): void {
  etagStores.get(queryClient)?.delete(hashKey(queryKey));
}

export async function fetchWithETag<T extends { version?: number }>(
  queryClient: QueryClient,
  queryKey: QueryKey,
  fetcher: (options?: {
    ifNoneMatch?: string;
  }) => Promise<ConditionalResponse<T>>,
  resourceLabel: string,
): Promise<T> {
  const previousData = queryClient.getQueryData<T>(queryKey);
  const cachedEtag =
    getCachedETag(queryClient, queryKey) ?? formatETag(previousData?.version);
  const ifNoneMatch = previousData !== undefined ? cachedEtag : undefined;

  const response = await fetcher(ifNoneMatch ? { ifNoneMatch } : undefined);

  if (response.notModified) {
    if (previousData !== undefined) {
      if (response.etag) {
        setCachedETag(queryClient, queryKey, response.etag);
      }
      return previousData;
    }

    const fallback = await fetcher();
    if (fallback.notModified) {
      throw new Error(
        `Received 304 Not Modified for ${resourceLabel} without cached data`,
      );
    }
    setCachedETag(
      queryClient,
      queryKey,
      fallback.etag ?? formatETag(fallback.data.version),
    );
    return fallback.data;
  }

  setCachedETag(
    queryClient,
    queryKey,
    response.etag ?? formatETag(response.data.version),
  );
  return response.data;
}
