"use client";

import * as React from "react";
import { useEffect } from "react";

import { CopilotKit } from "@copilotkit/react-core";
import { QueryClientProvider } from "@tanstack/react-query";
import { Analytics } from "@vercel/analytics/next";
import { useSetAtom } from "jotai";
import { ThemeProvider as NextThemesProvider } from "next-themes";

import { isCopilotEnabledAtom } from "@/atoms/feature-flags-atom";
import { BackToTop } from "@/components/back-to-top";
import { MobileBottomNav } from "@/components/mobile-bottom-nav";
import { env } from "@/env.mjs";
import { initMocks } from "@/lib/msw";
import { getQueryClient } from "@/lib/query-client";

export function Providers({
  children,
  isCopilotEnabled,
}: {
  children: React.ReactNode;
  isCopilotEnabled: boolean;
}) {
  const setIsCopilotEnabled = useSetAtom(isCopilotEnabledAtom);
  const queryClient = getQueryClient();
  const gatewayUrl =
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP;

  useEffect(() => {
    if (!gatewayUrl && process.env.NODE_ENV === "development") {
      initMocks();
    }
    setIsCopilotEnabled(isCopilotEnabled);
  }, [isCopilotEnabled, setIsCopilotEnabled, gatewayUrl]);

  const shouldShowCopilot = !!isCopilotEnabled && !!gatewayUrl;

  return (
    <QueryClientProvider client={queryClient}>
      <NextThemesProvider
        attribute="class"
        defaultTheme="light"
        disableTransitionOnChange
        enableColorScheme
      >
        {shouldShowCopilot ? (
          <CopilotKit
            runtimeUrl="/api/copilotkit"
            agent={env.NEXT_PUBLIC_COPILOT_AGENT_NAME}
          >
            <div className="pb-16 md:pb-0">{children}</div>
          </CopilotKit>
        ) : (
          <div className="pb-16 md:pb-0">{children}</div>
        )}
        <MobileBottomNav />
        <BackToTop />
        <Analytics />
      </NextThemesProvider>
    </QueryClientProvider>
  );
}
