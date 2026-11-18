"use client";

import { useEffect } from "react";

import { CopilotKitCore } from "@copilotkit/react-core/v2";

import { searchBooksTool } from "@/lib/webmcp-tools";

/**
 * Registers public storefront tools with WebMCP (`document.modelContext`)
 * without requiring a CopilotKit runtime or backend agent.
 *
 * Disabled when `CopilotKitProvider` is mounted so `useBookSearchActions`
 * owns the `searchBooks` registration without duplicates.
 */
export function useWebMCPTools(enabled: boolean) {
  useEffect(() => {
    if (!enabled) {
      return;
    }

    const core = new CopilotKitCore({
      tools: [searchBooksTool],
    });

    return () => {
      core.dispose();
    };
  }, [enabled]);
}
