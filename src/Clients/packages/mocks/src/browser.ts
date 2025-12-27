import { setupWorker } from "msw/browser";
import { authorsHandlers } from "./catalog/authors/index";
import { categoriesHandlers } from "./catalog/categories/index";
import { publishersHandlers } from "./catalog/publishers/index";
import { booksHandlers } from "./catalog/books/index.js";
import { basketHandlers } from "./basket/index";
import { buyersHandlers } from "./ordering/buyers/index";
import { ordersHandlers } from "./ordering/orders/index";
import { feedbacksHandlers } from "./rating/index";

export const worker = setupWorker(
  ...authorsHandlers,
  ...categoriesHandlers,
  ...publishersHandlers,
  ...booksHandlers,
  ...basketHandlers,
  ...buyersHandlers,
  ...ordersHandlers,
  ...feedbacksHandlers,
);

export const startMocking = async () => {
  await worker.start({
    onUnhandledRequest: "warn",
  });
};
