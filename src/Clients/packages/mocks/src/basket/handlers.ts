import { HttpResponse, http } from "msw";

import { formatValidationErrors } from "@workspace/utils/validation";
import {
  createBasketSchema,
  updateBasketSchema,
} from "@workspace/validations/basket";

import { BASKET_API_BASE_URL } from "./constants";
import { MOCK_USER_ID, basketStore } from "./data";

// Support both direct basket URL and gateway pattern
const basketPaths = [
  `${BASKET_API_BASE_URL}/api/v1/baskets`,
  "*/basket/api/v1/baskets", // Gateway pattern
];

export const basketHandlers = [
  ...basketPaths.map((path) =>
    http.get(path, () => {
      const basket = basketStore.get(MOCK_USER_ID);

      if (!basket) {
        return new HttpResponse(null, { status: 404 });
      }

      return HttpResponse.json(basket, { status: 200 });
    }),
  ),

  ...basketPaths.map((path) =>
    http.post(path, async ({ request }) => {
      const body = await request.json();
      const result = createBasketSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const basket = basketStore.createOrUpdate(
        MOCK_USER_ID,
        result.data.items.map((item) => ({
          id: item.id,
          quantity: item.quantity,
          name: `Book ${item.id.substring(0, 8)}`,
          price: 29.99,
          priceSale: null,
        })),
      );

      const headers = new Headers();
      headers.set("Location", `${BASKET_API_BASE_URL}/api/v1/baskets`);

      return HttpResponse.json(basket.id!, { status: 201, headers });
    }),
  ),

  ...basketPaths.map((path) =>
    http.put(path, async ({ request }) => {
      const body = await request.json();
      const result = updateBasketSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const existingBasket = basketStore.get(MOCK_USER_ID);

      if (!existingBasket) {
        return new HttpResponse(null, { status: 404 });
      }

      basketStore.createOrUpdate(
        MOCK_USER_ID,
        result.data.items.map((item) => ({
          id: item.id,
          quantity: item.quantity,
          name: `Book ${item.id.substring(0, 8)}`,
          price: 29.99,
          priceSale: null,
        })),
      );

      return new HttpResponse(null, { status: 204 });
    }),
  ),

  ...basketPaths.map((path) =>
    http.delete(path, () => {
      const deleted = basketStore.delete(MOCK_USER_ID);

      if (!deleted) {
        return new HttpResponse("Basket not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    }),
  ),
];
