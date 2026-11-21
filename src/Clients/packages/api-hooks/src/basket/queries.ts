import { queryOptions } from "@tanstack/react-query";

import basketApiClient from "@workspace/api-client/basket/baskets";

import { basketKeys } from "../keys";

export const basketQueries = {
  all: () => basketKeys.all,
  detail: () =>
    queryOptions({
      queryKey: basketKeys.detail(),
      queryFn: ({ signal }) => basketApiClient.get(signal),
    }),
};
