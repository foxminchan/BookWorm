import { createAuthRouteHandler } from "@workspace/auth/server";

import { auth } from "@/lib/auth";

export const { GET, POST } = createAuthRouteHandler(auth);
