import { headers } from "next/headers";
import type { NextRequest } from "next/server";

import { HttpAgent } from "@ag-ui/client";
import {
  CopilotRuntime,
  ExperimentalEmptyAdapter,
  copilotRuntimeNextJSAppRouterEndpoint,
} from "@copilotkit/runtime";
import { HttpStatusCode } from "axios";

import { env } from "@/env.mjs";

const serviceAdapter = new ExperimentalEmptyAdapter();

const getRuntime = async () => {
  const gatewayUrl =
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP;

  if (!gatewayUrl) {
    throw new Error("Gateway URL is not configured");
  }

  const agUiUrl = `${gatewayUrl}/chatting/ag-ui`;
  const agentName = env.NEXT_PUBLIC_COPILOT_AGENT_NAME;

  // Extract auth token from request headers to pass to backend agent
  const headersList = await headers();
  const authHeader = headersList.get("authorization");
  const cookieHeader = headersList.get("cookie");

  // Pass authentication context to backend agent via custom headers
  const agentHeaders: Record<string, string> = {};
  if (authHeader) {
    agentHeaders["authorization"] = authHeader;
  }
  if (cookieHeader) {
    agentHeaders["cookie"] = cookieHeader;
  }

  return new CopilotRuntime({
    [agentName]: new HttpAgent({
      url: agUiUrl,
      headers: agentHeaders,
    }),
  });
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
        status: HttpStatusCode.InternalServerError,
        headers: { "Content-Type": "application/json" },
      },
    );
  }
};
