import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { ordersApiClient } from "@workspace/api-client";
import type { Order, ListOrdersQuery } from "@workspace/types/ordering/orders";
import type { PagedResult } from "@workspace/types/shared";
import { orderingKeys } from "../../keys";

export function useOrders(
  query?: ListOrdersQuery,
  options?: Omit<UseQueryOptions<PagedResult<Order>>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: orderingKeys.orders.list(query),
    queryFn: () => ordersApiClient.listOrders(query),
    ...options,
  });
}
