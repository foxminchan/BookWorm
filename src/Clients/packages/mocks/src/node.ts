import { setupServer } from "msw/node";

import { basketHandlers } from "./basket/index";
import { authorsHandlers } from "./catalog/authors/index";
import { booksHandlers } from "./catalog/books/index.js";
import { categoriesHandlers } from "./catalog/categories/index";
import { publishersHandlers } from "./catalog/publishers/index";
import { buyersHandlers } from "./ordering/buyers/index";
import { ordersHandlers } from "./ordering/orders/index";
import { feedbacksHandlers } from "./rating/index";

export const server = setupServer(
  ...authorsHandlers,
  ...categoriesHandlers,
  ...publishersHandlers,
  ...booksHandlers,
  ...basketHandlers,
  ...buyersHandlers,
  ...ordersHandlers,
  ...feedbacksHandlers,
);

export const startServer = () => {
  server.listen({ onUnhandledRequest: "warn" });
};

export const stopServer = () => {
  server.close();
};

export const resetServer = () => {
  server.resetHandlers();
};
