"use client";

import { StorefrontErrorBoundary } from "@workspace/storefront-shell/components/error-boundary";

export default function ErrorBoundary(
  props: Readonly<{
    error: Error & { digest?: string };
    reset: () => void;
  }>,
) {
  return (
    <StorefrontErrorBoundary
      {...props}
      logPrefix="Commerce error boundary caught:"
    />
  );
}
