import { v7 as uuidv7 } from "uuid";
import type { CustomerBasket } from "@workspace/types/basket";

const basketsMap = new Map<string, CustomerBasket>();

export const MOCK_USER_ID = uuidv7();

basketsMap.set(MOCK_USER_ID, {
  id: MOCK_USER_ID,
  items: [],
});

export const basketStore = {
  getBasket: (userId: string = MOCK_USER_ID): CustomerBasket | undefined => {
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
    basketsMap.set(MOCK_USER_ID, {
      id: MOCK_USER_ID,
      items: [],
    });
  },
};
