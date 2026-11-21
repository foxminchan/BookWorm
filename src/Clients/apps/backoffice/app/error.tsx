"use client";

import { useEffect } from "react";

import Link from "next/link";

import { Button } from "@workspace/ui/components/button";

export default function ErrorBoundary({
  error,
  reset,
}: Readonly<{
  error: Error & { digest?: string };
  reset: () => void;
}>) {
  useEffect(() => {
    console.error("Backoffice error boundary caught:", error);
  }, [error]);

  return (
    <div className="bg-background flex min-h-screen items-center justify-center px-4">
      <div className="space-y-6 text-center">
        <div className="space-y-2">
          <h1 className="text-foreground text-3xl font-bold">
            Something went wrong
          </h1>
          <p className="text-foreground/60 max-w-md">
            An unexpected error occurred. Please try again or return to the
            dashboard.
          </p>
        </div>

        <div className="flex justify-center gap-3">
          <Button type="button" onClick={reset}>
            Try Again
          </Button>
          <Button
            render={<Link href="/">Go to Dashboard</Link>}
            nativeButton={false}
            variant="outline"
          />
        </div>
      </div>
    </div>
  );
}
