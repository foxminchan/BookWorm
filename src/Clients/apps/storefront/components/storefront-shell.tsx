import type React from "react";
import { Suspense } from "react";

import { Footer } from "@/components/footer";
import { Header } from "@/components/header";
import { MobileBottomNav } from "@/components/mobile-bottom-nav";

export function StorefrontShell({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="bg-background flex min-h-screen flex-col">
      <Suspense
        fallback={
          <div className="bg-background/80 sticky top-0 z-50 h-16 w-full border-b md:h-20" />
        }
      >
        <Header />
      </Suspense>
      <main className="flex-1" id="main-content">
        {children}
      </main>
      <Footer />
      <Suspense fallback={null}>
        <MobileBottomNav />
      </Suspense>
    </div>
  );
}
