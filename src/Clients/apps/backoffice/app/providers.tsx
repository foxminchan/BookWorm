"use client";

import type React from "react";
import { useEffect } from "react";

import { QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider as NextThemesProvider } from "next-themes";

import { getQueryClient } from "@workspace/api-hooks/query-client";
import { Toaster } from "@workspace/ui/components/sonner";
import { useAccessToken } from "@workspace/ui/hooks/useAccessToken";

import { env } from "@/env.mjs";
import { authClient, useSession } from "@/lib/auth-client";

export function Providers({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  const queryClient = getQueryClient();

  const gatewayUrl =
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP;

  useEffect(() => {
    if (!gatewayUrl && process.env.NODE_ENV === "development") {
      void import("@workspace/mocks/init")
        .then(({ initMocks }) => initMocks())
        .catch((error: unknown) => {
          console.error("Failed to initialize backoffice mocks:", error);
        });
    }
  }, [gatewayUrl]);

  useAccessToken(authClient, useSession);

  return (
    <QueryClientProvider client={queryClient}>
      <NextThemesProvider
        attribute="class"
        defaultTheme="dark"
        storageKey="backoffice-theme"
        enableSystem
        enableColorScheme
        disableTransitionOnChange
      >
        {children}
        <Toaster />
      </NextThemesProvider>
    </QueryClientProvider>
  );
}
