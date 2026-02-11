import { setupServer } from "msw/node";

import { allHandlers } from "./handlers";

export const server = setupServer(...allHandlers);

export const startServer = () => {
  server.listen({ onUnhandledRequest: "warn" });
};

export const stopServer = () => {
  server.close();
};

export const resetServer = () => {
  server.resetHandlers();
};
