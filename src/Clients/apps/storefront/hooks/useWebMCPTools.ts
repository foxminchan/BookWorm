"use client";

import { useEffect } from "react";

/**
 * Registers public storefront tools with WebMCP (`document.modelContext`)
 * without requiring a CopilotKit runtime or backend agent.
 *
 * Disabled when `CopilotKitProvider` is mounted so `useBookSearchActions`
 * owns the tool registrations without duplicates.
 */
export function useWebMCPTools(enabled: boolean) {
  useEffect(() => {
    if (!enabled) {
      return;
    }

    let cancelled = false;
    let core: { dispose: () => void } | undefined;

    void Promise.all([
      import("@copilotkit/react-core"),
      import("@/lib/webmcp-tools"),
    ])
      .then(([{ CopilotKitCore }, { getBookDetailsTool, searchBooksTool }]) => {
        if (cancelled) {
          return;
        }
        core = new CopilotKitCore({
          tools: [searchBooksTool, getBookDetailsTool],
        });
      })
      .catch((error: unknown) => {
        console.error("Failed to initialize WebMCP tools:", error);
      });

    return () => {
      cancelled = true;
      core?.dispose();
    };
  }, [enabled]);
}
