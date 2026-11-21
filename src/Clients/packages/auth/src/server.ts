import { toNextJsHandler } from "better-auth/next-js";

export {
  createEntraAuth,
  type CreateEntraAuthOptions,
  type EntraAuthEnv,
} from "@workspace/utils/auth";

/** Creates Next.js App Router GET/POST route handlers for a Better Auth instance. */
export const createAuthRouteHandler = toNextJsHandler;
