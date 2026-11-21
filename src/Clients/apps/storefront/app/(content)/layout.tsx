import type React from "react";

import { StorefrontShell } from "@/components/storefront-shell";

export default function ContentLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return <StorefrontShell>{children}</StorefrontShell>;
}
