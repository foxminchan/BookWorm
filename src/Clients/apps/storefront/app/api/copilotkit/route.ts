import type { NextRequest } from "next/server";

import { HttpAgent } from "@ag-ui/client";
import {
  CopilotRuntime,
  ExperimentalEmptyAdapter,
  copilotRuntimeNextJSAppRouterEndpoint,
} from "@copilotkit/runtime";

import { env } from "@/env.mjs";

const serviceAdapter = new ExperimentalEmptyAdapter();

const getRuntime = () => {
  const gatewayUrl =
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP;

  if (!gatewayUrl) {
    throw new Error("Gateway URL is not configured");
  }

  const agUiUrl = `${gatewayUrl}/chatting/ag-ui`;

  return new CopilotRuntime({
    agents: {
      "chat-workflow": new HttpAgent({ url: agUiUrl }),
    },
  });
};

export const POST = async (req: NextRequest) => {
  try {
    const runtime = getRuntime();
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
