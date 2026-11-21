import {
  type UseQueryOptions,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";

import { formatETag } from "@workspace/api-client/client";
import ordersApiClient from "@workspace/api-client/ordering/orders";
import type {
  OrderSummaryQuery,
  OrderSummarySnapshot,
} from "@workspace/types/ordering/orders";

import { fetchWithETag, getCachedETag } from "../../etag-cache";
import { orderingKeys } from "../../keys";

export default function useOrderSummary(
  id: string,
  query?: OrderSummaryQuery,
  options?: Omit<UseQueryOptions<OrderSummarySnapshot>, "queryKey" | "queryFn">,
) {
  const queryClient = useQueryClient();
  const queryKey = orderingKeys.orders.summary(id, query);

  const result = useQuery({
    queryKey,
    queryFn: () =>
      fetchWithETag(
        queryClient,
        queryKey,
        (opts) => ordersApiClient.getSummaryConditional(id, query, opts),
        `order summary ${id}`,
      ),
    ...options,
  });

  const etag =
    getCachedETag(queryClient, queryKey) ?? formatETag(result.data?.version);

  return { ...result, etag };
}
