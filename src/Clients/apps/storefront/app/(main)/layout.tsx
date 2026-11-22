import type React from "react";

import { StorefrontShell } from "@workspace/storefront-shell/components/storefront-shell";

export default function MainLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return <StorefrontShell>{children}</StorefrontShell>;
}
