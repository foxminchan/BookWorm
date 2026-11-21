"use client";

import * as React from "react";
import { useEffect } from "react";

import dynamic from "next/dynamic";

import { type QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Provider as JotaiProvider, useSetAtom } from "jotai";
import { queryClientAtom } from "jotai-tanstack-query";
import { useHydrateAtoms } from "jotai/utils";
import { ThemeProvider as NextThemesProvider } from "next-themes";

import { useAccessToken } from "@workspace/ui/hooks/useAccessToken";

import { isAuthenticatedAtom } from "@/atoms/basket-atom";
import { isCopilotEnabledAtom } from "@/atoms/feature-flags-atom";
import { BackToTop } from "@/components/back-to-top";
import { env } from "@/env.mjs";
import { useWebMCPTools } from "@/hooks/useWebMCPTools";
import { authClient, useSession } from "@/lib/auth-client";
import { COPILOT_RUNTIME_URL } from "@/lib/constants";
import { getQueryClient } from "@/lib/query-client";

const CopilotKitProvider = dynamic(
  () => import("@copilotkit/react-core/v2").then((m) => m.CopilotKitProvider),
  { ssr: false },
);

function HydrateAtoms({
  queryClient,
  children,
}: Readonly<{
  queryClient: QueryClient;
  children: React.ReactNode;
}>) {
  useHydrateAtoms([[queryClientAtom, queryClient]]);
  return children;
}

function ProvidersContent({
  children,
  isCopilotEnabled,
  queryClient,
}: Readonly<{
  children: React.ReactNode;
  isCopilotEnabled: boolean;
  queryClient: QueryClient;
}>) {
  const setIsCopilotEnabled = useSetAtom(isCopilotEnabledAtom);
  const setIsAuthenticated = useSetAtom(isAuthenticatedAtom);
  const { data: session } = useSession();
  const gatewayUrl =
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP;

  useEffect(() => {
    if (!gatewayUrl && process.env.NODE_ENV === "development") {
      void import("@workspace/mocks/init")
        .then(({ initMocks }) => initMocks())
        .catch((error: unknown) => {
          console.error("Failed to initialize storefront mocks:", error);
        });
    }
    setIsCopilotEnabled(isCopilotEnabled);
  }, [isCopilotEnabled, setIsCopilotEnabled, gatewayUrl]);

  useEffect(() => {
    setIsAuthenticated(!!session?.user);
  }, [session, setIsAuthenticated]);

  useAccessToken(authClient, useSession);

  // The chat agent (AG-UI) requires an authenticated user
  const shouldShowCopilot =
    !!isCopilotEnabled && !!gatewayUrl && !!session?.user;

  useWebMCPTools(!shouldShowCopilot);

  return (
    <QueryClientProvider client={queryClient}>
      <NextThemesProvider
        attribute="class"
        defaultTheme="light"
        disableTransitionOnChange
        enableColorScheme
      >
        {shouldShowCopilot ? (
          <CopilotKitProvider runtimeUrl={COPILOT_RUNTIME_URL}>
            <div className="pb-16 md:pb-0">{children}</div>
          </CopilotKitProvider>
        ) : (
          <div className="pb-16 md:pb-0">{children}</div>
        )}
        <BackToTop />
      </NextThemesProvider>
    </QueryClientProvider>
  );
}

export function Providers({
  children,
  isCopilotEnabled,
}: Readonly<{
  children: React.ReactNode;
  isCopilotEnabled: boolean;
}>) {
  const queryClient = getQueryClient();

  return (
    <JotaiProvider>
      <HydrateAtoms queryClient={queryClient}>
        <ProvidersContent
          isCopilotEnabled={isCopilotEnabled}
          queryClient={queryClient}
        >
          {children}
        </ProvidersContent>
      </HydrateAtoms>
    </JotaiProvider>
  );
}
