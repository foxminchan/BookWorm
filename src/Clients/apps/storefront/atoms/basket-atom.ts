"use client";

import { atom } from "jotai";
import { atomWithQuery } from "jotai-tanstack-query";

import basketApiClient from "@workspace/api-client/basket/baskets";

export const isAuthenticatedAtom = atom<boolean>(false);

export const basketAtom = atomWithQuery((get) => ({
  queryKey: ["basket", "detail"],
  queryFn: () => basketApiClient.get(),
  enabled: get(isAuthenticatedAtom),
}));

export const basketItemCountAtom = atom((get) => {
  const result = get(basketAtom);
  if (!result.data?.items) return 0;
  return result.data.items.reduce((sum, item) => sum + item.quantity, 0);
});

export const basketItemsAtom = atom((get) => {
  const result = get(basketAtom);
  return result.data?.items ?? [];
});
