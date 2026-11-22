"use client";

import { useEffect } from "react";

import { useAgent } from "@copilotkit/react-core/v2";
import { bindAgent } from "@generative-a11y/ag-ui";
import { useRuntime } from "@generative-a11y/react";

/**
 * Bridges a CopilotKit v2 AG-UI agent into the generative-a11y runtime
 * for paced screen-reader announcements of streaming text, tool calls, and interrupts.
 */
export function CopilotKitAccessibility({
  agentId,
}: Readonly<{ agentId: string }>) {
  const { agent } = useAgent({ agentId });
  const runtime = useRuntime();

  useEffect(() => {
    const binding = bindAgent({
      runtime,
      scopeId: agentId,
      agent,
    });

    return () => {
      binding.dispose();
    };
  }, [runtime, agentId, agent]);

  return null;
}
