import { setupWorker } from "msw/browser";

import { allHandlers } from "./handlers";

export const worker = setupWorker(...allHandlers);

export const startMocking = async () => {
  await worker.start({
    onUnhandledFrame: "warn",
    serviceWorker: {
      url: "/mockServiceWorker.js",
    },
    quiet: false,
  });
};
