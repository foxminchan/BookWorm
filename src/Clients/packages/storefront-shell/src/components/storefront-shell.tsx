import type React from "react";
import { Suspense } from "react";

import { PrefetchCrossZoneLinks } from "@vercel/microfrontends/next/client";

import { Footer } from "./footer";
import { Header } from "./header";
import { MobileBottomNav } from "./mobile-bottom-nav";

export type StorefrontShellProps = {
  readonly children: React.ReactNode;
  readonly hardNavigateSearch?: boolean;
};

export function StorefrontShell({
  children,
  hardNavigateSearch,
}: StorefrontShellProps) {
  return (
    <div className="bg-background flex min-h-screen flex-col">
      <Suspense
        fallback={
          <div className="bg-background/80 sticky top-0 z-50 h-16 w-full border-b md:h-20" />
        }
      >
        <Header hardNavigateSearch={hardNavigateSearch} />
      </Suspense>
      <main className="flex-1" id="main-content">
        {children}
      </main>
      <Footer />
      <Suspense fallback={null}>
        <MobileBottomNav />
      </Suspense>
      <PrefetchCrossZoneLinks />
    </div>
  );
}
