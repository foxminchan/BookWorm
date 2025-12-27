import { setupWorker } from "msw/browser";
import { authorsHandlers } from "./catalog/authors/index";
import { categoriesHandlers } from "./catalog/categories/index";
import { publishersHandlers } from "./catalog/publishers/index";
import { booksHandlers } from "./catalog/books/index.js";

export const worker = setupWorker(
  ...authorsHandlers,
  ...categoriesHandlers,
  ...publishersHandlers,
  ...booksHandlers,
);

export const startMocking = async () => {
  await worker.start({
    onUnhandledRequest: "warn",
  });
};
