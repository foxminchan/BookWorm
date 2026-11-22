import type React from "react";

import { StorefrontShell } from "@workspace/storefront-shell/components/storefront-shell";

export default function OrderingLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return <StorefrontShell hardNavigateSearch>{children}</StorefrontShell>;
}
