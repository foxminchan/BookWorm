"use client";

import * as React from "react";
import { ThemeProvider as NextThemesProvider } from "next-themes";
import { useEffect, useState } from "react";
import { CopilotKit } from "@copilotkit/react-core";
import { Provider } from "jotai";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Analytics } from "@vercel/analytics/next";
import { BackToTop } from "@/components/back-to-top";
import { MobileBottomNav } from "@/components/mobile-bottom-nav";
import { initMocks } from "@/lib/msw";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000,
      refetchOnWindowFocus: false,
    },
  },
});

const gatewayUrl =
  process.env.NEXT_PUBLIC_GATEWAY_HTTPS || process.env.NEXT_PUBLIC_GATEWAY_HTTP;
const copilotKitUrl = gatewayUrl ? `${gatewayUrl}/chat/ag-ui` : undefined;
const shouldRenderCopilotKit = !!copilotKitUrl;

export function Providers({ children }: { children: React.ReactNode }) {
  const [mswReady, setMswReady] = useState(false);

  useEffect(() => {
    initMocks().then(() => setMswReady(true));
  }, []);

  if (!mswReady && process.env.NODE_ENV === "development") {
    return null;
  }

  return shouldRenderCopilotKit ? (
    <NextThemesProvider
      attribute="class"
      defaultTheme="system"
      enableSystem
      disableTransitionOnChange
      enableColorScheme
    >
      <CopilotKit runtimeUrl={copilotKitUrl}>
        <QueryClientProvider client={queryClient}>
          <Provider>
            <div className="md:pb-0 pb-16">{children}</div>
            <MobileBottomNav />
            <BackToTop />
            <Analytics />
          </Provider>
        </QueryClientProvider>
      </CopilotKit>
    </NextThemesProvider>
  ) : (
    <NextThemesProvider
      attribute="class"
      defaultTheme="system"
      enableSystem
      disableTransitionOnChange
      enableColorScheme
    >
      <QueryClientProvider client={queryClient}>
        <Provider>
          <div className="md:pb-0 pb-16">{children}</div>
          <MobileBottomNav />
          <BackToTop />
          <Analytics />
        </Provider>
      </QueryClientProvider>
    </NextThemesProvider>
  );
}
