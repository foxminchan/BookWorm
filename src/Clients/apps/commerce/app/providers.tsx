"use client";

import * as React from "react";
import { useEffect } from "react";

import { type QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { PrefetchCrossZoneLinksProvider } from "@vercel/microfrontends/next/client";
import { Provider as JotaiProvider, useSetAtom } from "jotai";
import { queryClientAtom } from "jotai-tanstack-query";
import { useHydrateAtoms } from "jotai/utils";
import { ThemeProvider as NextThemesProvider } from "next-themes";

import { isAuthenticatedAtom } from "@workspace/storefront-shell/atoms/basket-atom";
import { BackToTop } from "@workspace/storefront-shell/components/back-to-top";
import { useAccessToken } from "@workspace/ui/hooks/useAccessToken";

import { env } from "@/env.mjs";
import { authClient, useSession } from "@/lib/auth-client";
import { getQueryClient } from "@/lib/query-client";

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
  queryClient,
}: Readonly<{
  children: React.ReactNode;
  queryClient: QueryClient;
}>) {
  const setIsAuthenticated = useSetAtom(isAuthenticatedAtom);
  const { data: session } = useSession();
  const gatewayUrl =
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP;

  useEffect(() => {
    if (!gatewayUrl && process.env.NODE_ENV === "development") {
      void import("@workspace/mocks/init")
        .then(({ initMocks }) => initMocks())
        .catch((error: unknown) => {
          console.error("Failed to initialize commerce mocks:", error);
        });
    }
  }, [gatewayUrl]);

  useEffect(() => {
    setIsAuthenticated(!!session?.user);
  }, [session, setIsAuthenticated]);

  useAccessToken(authClient, useSession);

  return (
    <QueryClientProvider client={queryClient}>
      <NextThemesProvider
        attribute="class"
        defaultTheme="light"
        disableTransitionOnChange
        enableColorScheme
      >
        <PrefetchCrossZoneLinksProvider>
          <div className="pb-16 md:pb-0">{children}</div>
          <BackToTop />
        </PrefetchCrossZoneLinksProvider>
      </NextThemesProvider>
    </QueryClientProvider>
  );
}

export function Providers({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const queryClient = getQueryClient();

  return (
    <JotaiProvider>
      <HydrateAtoms queryClient={queryClient}>
        <ProvidersContent queryClient={queryClient}>
          {children}
        </ProvidersContent>
      </HydrateAtoms>
    </JotaiProvider>
  );
}
