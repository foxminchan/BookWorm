"use client";

import type React from "react";
import { useEffect } from "react";

import { QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider as NextThemesProvider } from "next-themes";

import { Toaster } from "@workspace/ui/components/sonner";

import { env } from "@/env.mjs";
import { initMocks } from "@/lib/msw";
import { getQueryClient } from "@/lib/query-client";

export function Providers({ children }: Readonly<{ children: React.ReactNode }>) {
  const queryClient = getQueryClient();

  const gatewayUrl =
    env.NEXT_PUBLIC_GATEWAY_HTTPS || env.NEXT_PUBLIC_GATEWAY_HTTP;

  useEffect(() => {
    if (!gatewayUrl && process.env.NODE_ENV === "development") {
      initMocks();
    }
  }, [gatewayUrl]);

  return (
    <QueryClientProvider client={queryClient}>
      <NextThemesProvider
        attribute="class"
        defaultTheme="dark"
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
