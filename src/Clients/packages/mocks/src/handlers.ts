import type { RequestHandler } from "msw";

import { basketHandlers } from "./basket/index";
import { authorsHandlers } from "./catalog/authors/index";
import { booksHandlers } from "./catalog/books/index";
import { categoriesHandlers } from "./catalog/categories/index";
import { publishersHandlers } from "./catalog/publishers/index";
import { buyersHandlers } from "./ordering/buyers/index";
import { ordersHandlers } from "./ordering/orders/index";
import { feedbacksHandlers } from "./rating/index";

/** All MSW request handlers aggregated from every service domain. */
export const allHandlers: RequestHandler[] = [
  ...authorsHandlers,
  ...categoriesHandlers,
  ...publishersHandlers,
  ...booksHandlers,
  ...basketHandlers,
  ...buyersHandlers,
  ...ordersHandlers,
  ...feedbacksHandlers,
];
