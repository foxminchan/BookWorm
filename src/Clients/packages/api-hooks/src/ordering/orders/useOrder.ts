import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { ordersApiClient } from "@workspace/api-client";
import type { OrderDetail } from "@workspace/types/ordering/orders";
import { orderingKeys } from "../../keys";

export function useOrder(
  id: string,
  options?: Omit<UseQueryOptions<OrderDetail>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: orderingKeys.orders.detail(id),
    queryFn: () => ordersApiClient.getOrder(id),
    ...options,
  });
}
