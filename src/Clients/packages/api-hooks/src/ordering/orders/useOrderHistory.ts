import { type UseQueryOptions, useQuery } from "@tanstack/react-query";

import ordersApiClient from "@workspace/api-client/ordering/orders";
import type { OrderEventHistory } from "@workspace/types/ordering/orders";

import { orderingKeys } from "../../keys";

export default function useOrderHistory(
  id: string,
  options?: Omit<UseQueryOptions<OrderEventHistory[]>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: orderingKeys.orders.history(id),
    queryFn: () => ordersApiClient.getHistory(id),
    ...options,
  });
}
