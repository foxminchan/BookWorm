import { type UseQueryOptions, useQuery } from "@tanstack/react-query";

import ordersApiClient from "@workspace/api-client/ordering/orders";
import type {
  OrderSummaryQuery,
  OrderSummarySnapshot,
} from "@workspace/types/ordering/orders";

import { orderingKeys } from "../../keys";

export default function useOrderSummary(
  id: string,
  query?: OrderSummaryQuery,
  options?: Omit<UseQueryOptions<OrderSummarySnapshot>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: orderingKeys.orders.summary(id, query),
    queryFn: () => ordersApiClient.getSummary(id, query),
    ...options,
  });
}
