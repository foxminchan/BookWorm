export {
  createEntraAuth,
  type CreateEntraAuthOptions,
  type EntraAuthEnv,
} from "@workspace/utils/auth";

/** Creates Next.js App Router GET/POST route handlers for a Better Auth instance. */
export { toNextJsHandler as createAuthRouteHandler } from "better-auth/next-js";
