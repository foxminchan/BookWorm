import { headers } from "next/headers";
import type { NextRequest } from "next/server";

import { HttpAgent } from "@ag-ui/client";
import {
  CopilotRuntime,
  ExperimentalEmptyAdapter,
  copilotRuntimeNextJSAppRouterEndpoint,
} from "@copilotkit/runtime";

import { AUTH } from "@workspace/utils/constants";

import { env } from "@/env.mjs";
import { auth } from "@/lib/auth";

const serviceAdapter = new ExperimentalEmptyAdapter();

const jsonResponse = (status: number, body: Record<string, unknown>) =>
  new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });

/**
 * Resolves the Keycloak access token for the current Better Auth session.
 * Better Auth refreshes the token automatically when it has expired.
 */
const getAccessToken = async (
  requestHeaders: Headers,
): Promise<string | null> => {
  try {
    const session = await auth.api.getSession({ headers: requestHeaders });
    if (!session?.user) {
      return null;
    }

    const accounts = await auth.api.listUserAccounts({
      headers: requestHeaders,
    });
    const account = accounts?.find(
      (account) => account.providerId === AUTH.PROVIDER,
    );
    if (!account) {
      return null;
    }

    const token = await auth.api.getAccessToken({
      body: { accountId: account.id },
      headers: requestHeaders,
    });

    return token?.accessToken ?? null;
  } catch (error) {
    console.error("Failed to resolve access token for chat:", error);
    return null;
  }
};

const getRuntime = (accessToken: string) => {
  const gatewayUrl =
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP;

  if (!gatewayUrl) {
    throw new Error("Gateway URL is not configured");
  }

  const agUiUrl = `${gatewayUrl}/chatting/ag-ui`;
  const agentName = env.NEXT_PUBLIC_COPILOT_AGENT_NAME;

  const agents = {
    [agentName]: new HttpAgent({
      url: agUiUrl,
      headers: { authorization: `Bearer ${accessToken}` },
    }),
  };

  return new CopilotRuntime({ agents, a2ui: true } as any);
};

export const POST = async (req: NextRequest) => {
  const accessToken = await getAccessToken(await headers());

  if (!accessToken) {
    return jsonResponse(401, {
      error: "Unauthorized",
      message: "You must be signed in to use the chat assistant.",
    });
  }

  try {
    const runtime = getRuntime(accessToken);
    const { handleRequest } = copilotRuntimeNextJSAppRouterEndpoint({
      runtime,
      serviceAdapter,
      endpoint: "/api/copilotkit",
    });

    return handleRequest(req);
  } catch (error) {
    console.error("CopilotKit runtime error:", error);
    return jsonResponse(500, {
      error: "Failed to process request",
      message:
        error instanceof Error ? error.message : "Unknown error occurred",
    });
  }
};
