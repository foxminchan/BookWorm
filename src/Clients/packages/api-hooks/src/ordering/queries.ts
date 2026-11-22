import { queryOptions } from "@tanstack/react-query";

import buyersApiClient from "@workspace/api-client/ordering/buyers";
import ordersApiClient from "@workspace/api-client/ordering/orders";
import type { ListBuyersQuery } from "@workspace/types/ordering/buyers";
import type { ListOrdersQuery } from "@workspace/types/ordering/orders";

import { orderingKeys } from "../keys";

export const orderQueries = {
  all: () => orderingKeys.orders.all,
  list: (query?: ListOrdersQuery) =>
    queryOptions({
      queryKey: orderingKeys.orders.list(query),
      queryFn: ({ signal }) => ordersApiClient.list(query, signal),
    }),
  detail: (id: string) =>
    queryOptions({
      queryKey: orderingKeys.orders.detail(id),
      queryFn: ({ signal }) => ordersApiClient.get(id, signal),
    }),
  history: (id: string) =>
    queryOptions({
      queryKey: orderingKeys.orders.history(id),
      queryFn: ({ signal }) => ordersApiClient.getHistory(id, signal),
    }),
};

export const buyerQueries = {
  all: () => orderingKeys.buyers.all,
  current: () =>
    queryOptions({
      queryKey: orderingKeys.buyers.current(),
      queryFn: ({ signal }) => buyersApiClient.getCurrentBuyer(signal),
    }),
  list: (query?: ListBuyersQuery) =>
    queryOptions({
      queryKey: orderingKeys.buyers.list(query),
      queryFn: ({ signal }) => buyersApiClient.list(query, signal),
    }),
};
