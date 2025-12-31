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

export type BasketItemRequest = {
  id: string;
  quantity: number;
};

export type CreateBasketRequest = {
  items: BasketItemRequest[];
};

export type UpdateBasketRequest = {
  items: BasketItemRequest[];
};
