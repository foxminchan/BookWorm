"use client";

import { useEffect } from "react";

import { Link } from "@vercel/microfrontends/next/client";
import { Home, RotateCcw } from "lucide-react";

import { Button } from "@workspace/ui/components/button";

export default function ErrorBoundary({
  error,
  reset,
}: Readonly<{
  error: Error & { digest?: string };
  reset: () => void;
}>) {
  useEffect(() => {
    console.error("Storefront error boundary caught:", error);
  }, [error]);

  return (
    <div className="bg-background flex min-h-screen items-center justify-center px-4">
      <div className="w-full max-w-2xl space-y-8 text-center">
        <div className="space-y-4">
          <h1 className="text-foreground font-serif text-4xl font-medium md:text-5xl">
            Something Went Wrong
          </h1>
          <p className="text-muted-foreground mx-auto max-w-md text-lg leading-relaxed">
            An unexpected error occurred while loading this page. Please try
            again or return home.
          </p>
        </div>
        <div className="flex flex-col items-center justify-center gap-4 pt-6 sm:flex-row">
          <Button
            type="button"
            size="lg"
            onClick={reset}
            className="shadow-primary/20 gap-2 rounded-full px-8 shadow-lg"
          >
            <RotateCcw className="size-5" aria-hidden="true" />
            Try Again
          </Button>
          <Button
            render={
              <Link href="/">
                <Home className="size-5" aria-hidden="true" />
                Return Home
              </Link>
            }
            nativeButton={false}
            variant="outline"
            size="lg"
            className="gap-2 rounded-full bg-transparent px-8"
          />
        </div>
      </div>
    </div>
  );
}
