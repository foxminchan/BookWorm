import type { CustomerBasket } from "@workspace/types/basket";

import basketsData from "../data/baskets.json";

const mockBaskets: CustomerBasket[] = basketsData;

const basketsMap = new Map<string, CustomerBasket>(
  mockBaskets.map((basket) => [basket.id!, basket]),
);

export const MOCK_USER_ID = mockBaskets[0]?.id || "";

export const basketStore = {
  get: (userId: string = MOCK_USER_ID): CustomerBasket | undefined => {
    return basketsMap.get(userId);
  },

  createOrUpdate: (
    userId: string,
    items: CustomerBasket["items"],
  ): CustomerBasket => {
    const basket: CustomerBasket = {
      id: userId,
      items: items.map((item) => ({
        ...item,
        name: item.name || `Book ${item.id.substring(0, 8)}`,
        price: item.price || 29.99,
        priceSale: item.priceSale,
      })),
    };
    basketsMap.set(userId, basket);
    return basket;
  },

  delete: (userId: string = MOCK_USER_ID): boolean => {
    return basketsMap.delete(userId);
  },

  reset: () => {
    basketsMap.clear();
    mockBaskets.forEach((basket) => {
      if (basket.id) {
        basketsMap.set(basket.id, basket);
      }
    });
  },
};
