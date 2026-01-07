"use client";

import { useCopilotReadable } from "@copilotkit/react-core";
import { useAtomValue } from "jotai";

import { basketItemCountAtom, basketItemsAtom } from "@/atoms/basket-atom";

export function useBasketContext() {
  const basketItems = useAtomValue(basketItemsAtom);
  const itemCount = useAtomValue(basketItemCountAtom);

  const basketData = {
    itemCount,
    items: basketItems.map((item) => ({
      id: item.id,
      name: item.name,
      quantity: item.quantity,
      price: item.price,
      priceSale: item.priceSale,
    })),
    totalPrice: basketItems.reduce(
      (sum, item) => sum + (item.priceSale || item.price) * item.quantity,
      0,
    ),
  };

  useCopilotReadable({
    description:
      "The current user's shopping basket with books they plan to purchase",
    value: basketData,
  });
}
