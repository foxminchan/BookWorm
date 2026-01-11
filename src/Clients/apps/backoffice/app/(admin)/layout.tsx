"use client";

import type React from "react";

import Link from "next/link";

import { AuthGuard } from "@/components/auth-guard";
import { DashboardHeader } from "@/components/dashboard-header";
import { DashboardNav } from "@/components/dashboard-nav";
import { MobileBlocker } from "@/components/mobile-blocker";

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AuthGuard>
      <MobileBlocker>
        <Link
          href="#main-content"
          className="focus:bg-primary focus:text-primary-foreground focus:ring-ring sr-only focus:not-sr-only focus:absolute focus:top-4 focus:left-4 focus:z-50 focus:rounded-md focus:px-4 focus:py-2 focus:ring-2 focus:ring-offset-2 focus:outline-none"
        >
          Skip to main content
        </Link>
        <div className="bg-background flex h-screen">
          <DashboardNav />

          <div className="flex flex-1 flex-col overflow-hidden">
            <DashboardHeader />
            <main
              id="main-content"
              className="bg-background flex-1 overflow-auto p-6"
              tabIndex={-1}
            >
              {children}
            </main>
          </div>
        </div>
      </MobileBlocker>
    </AuthGuard>
  );
}
