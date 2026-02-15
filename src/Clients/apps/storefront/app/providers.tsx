"use client";

import * as React from "react";
import { useEffect } from "react";

import { CopilotKit } from "@copilotkit/react-core";
import { QueryClientProvider } from "@tanstack/react-query";
import { useSetAtom } from "jotai";
import { ThemeProvider as NextThemesProvider } from "next-themes";

import { useAccessToken } from "@workspace/ui/hooks/useAccessToken";

import { isAuthenticatedAtom } from "@/atoms/basket-atom";
import { isCopilotEnabledAtom } from "@/atoms/feature-flags-atom";
import { BackToTop } from "@/components/back-to-top";
import { MobileBottomNav } from "@/components/mobile-bottom-nav";
import { env } from "@/env.mjs";
import { authClient, useSession } from "@/lib/auth-client";
import { initMocks } from "@/lib/msw";
import { getQueryClient } from "@/lib/query-client";

export function Providers({
  children,
  isCopilotEnabled,
}: Readonly<{
  children: React.ReactNode;
  isCopilotEnabled: boolean;
}>) {
  const setIsCopilotEnabled = useSetAtom(isCopilotEnabledAtom);
  const setIsAuthenticated = useSetAtom(isAuthenticatedAtom);
  const { data: session } = useSession();
  const queryClient = getQueryClient();
  const gatewayUrl =
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP;

  useEffect(() => {
    if (!gatewayUrl && process.env.NODE_ENV === "development") {
      initMocks();
    }
    setIsCopilotEnabled(isCopilotEnabled);
  }, [isCopilotEnabled, setIsCopilotEnabled, gatewayUrl]);

  useEffect(() => {
    setIsAuthenticated(!!session?.user);
  }, [session, setIsAuthenticated]);

  useAccessToken(authClient, useSession);

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
      </NextThemesProvider>
    </QueryClientProvider>
  );
}
