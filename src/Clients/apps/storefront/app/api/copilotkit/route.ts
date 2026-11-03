import { headers } from "next/headers";
import type { NextRequest } from "next/server";

import { HttpAgent } from "@ag-ui/client";
import {
  CopilotRuntime,
  ExperimentalEmptyAdapter,
  copilotRuntimeNextJSAppRouterEndpoint,
} from "@copilotkit/runtime";

import { env } from "@/env.mjs";
import { auth } from "@/lib/auth";

const KEYCLOAK_PROVIDER_ID = "keycloak";
const serviceAdapter = new ExperimentalEmptyAdapter();

const getKeycloakAccessToken = async (
  requestHeaders: Headers,
): Promise<string | null> => {
  try {
    const accounts = await auth.api.listUserAccounts({
      headers: requestHeaders,
    });
    const account = accounts.find(
      (account) => account.providerId === KEYCLOAK_PROVIDER_ID,
    );

    if (!account) {
      return null;
    }

    const { accessToken } = await auth.api.getAccessToken({
      body: { accountId: account.id },
      headers: requestHeaders,
    });

    return accessToken ?? null;
  } catch {
    return null;
  }
};

const getRuntime = async () => {
  const gatewayUrl =
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP;

  if (!gatewayUrl) {
    throw new Error("Gateway URL is not configured");
  }

  const agUiUrl = `${gatewayUrl}/chatting/ag-ui`;
  const agentName = env.NEXT_PUBLIC_COPILOT_AGENT_NAME;

  // The AG-UI endpoint requires a Keycloak bearer token; resolve it from the
  // Better Auth session when the browser didn't send one explicitly.
  const headersList = await headers();
  const cookieHeader = headersList.get("cookie");
  const accessToken = await getKeycloakAccessToken(headersList);
  const authHeader =
    headersList.get("authorization") ??
    (accessToken ? `Bearer ${accessToken}` : null);

  const agentHeaders: Record<string, string> = {};
  if (authHeader) {
    agentHeaders["authorization"] = authHeader;
  }
  if (cookieHeader) {
    agentHeaders["cookie"] = cookieHeader;
  }

  const agents = {
    [agentName]: new HttpAgent({
      url: agUiUrl,
      headers: agentHeaders,
    }),
  };

  return new CopilotRuntime({ agents, a2ui: true } as any);
};

export const POST = async (req: NextRequest) => {
  try {
    const runtime = await getRuntime();
    const { handleRequest } = copilotRuntimeNextJSAppRouterEndpoint({
      runtime,
      serviceAdapter,
      endpoint: "/api/copilotkit",
    });

    return handleRequest(req);
  } catch (error) {
    console.error("CopilotKit runtime error:", error);
    return new Response(
      JSON.stringify({
        error: "Failed to process request",
        message:
          error instanceof Error ? error.message : "Unknown error occurred",
      }),
      {
        status: 500,
        headers: { "Content-Type": "application/json" },
      },
    );
  }
};
