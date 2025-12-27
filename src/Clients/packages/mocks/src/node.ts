import { setupServer } from "msw/node";
import { authorsHandlers } from "./catalog/authors/index";
import { categoriesHandlers } from "./catalog/categories/index";
import { publishersHandlers } from "./catalog/publishers/index";
import { booksHandlers } from "./catalog/books/index.js";

export const server = setupServer(
  ...authorsHandlers,
  ...categoriesHandlers,
  ...publishersHandlers,
  ...booksHandlers,
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
