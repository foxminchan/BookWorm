export type BasketItem = {
  id: string;
  quantity: number;
  name?: string;
  price: number;
  priceSale?: number | null;
};

export type CustomerBasket = {
  id?: string;
  items: BasketItem[];
};

export type CreateBasketRequest = {
  items: Array<{ id: string; quantity: number }>;
};

export type UpdateBasketRequest = {
  items: Array<{ id: string; quantity: number }>;
};
