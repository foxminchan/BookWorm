"use client";

import { useEffect } from "react";

import { useRouter } from "next/navigation";

import { Card, CardContent } from "@workspace/ui/components/card";
import { Spinner } from "@workspace/ui/components/spinner";

import { useUserContext } from "@/hooks/use-user-context";
import { signIn } from "@/lib/auth-client";
import { AUTH } from "@/lib/constants";

export function AuthGuard({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useUserContext();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      signIn.social({
        provider: AUTH.PROVIDER,
        callbackURL: AUTH.CALLBACK_URL,
      });
    }
  }, [isAuthenticated, isLoading, router]);

  if (isLoading) {
    return (
      <div className="from-background to-muted/20 flex h-screen items-center justify-center bg-linear-to-br">
        <Card
          className="border-muted/50 w-full max-w-md shadow-lg"
          role="status"
          aria-live="polite"
          aria-busy="true"
        >
          <CardContent className="space-y-6 pt-6 pb-6">
            <div className="flex justify-center" aria-hidden="true">
              <Spinner className="size-8" />
            </div>
            <div className="space-y-2 text-center">
              <h1 className="text-foreground text-xl font-semibold">
                Loading Dashboard
              </h1>
              <p className="text-muted-foreground text-sm">
                Please wait while we verify your credentials...
              </p>
            </div>
            <span className="sr-only">Loading dashboard, please wait</span>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="from-background to-muted/20 flex h-screen items-center justify-center bg-linear-to-br">
        <Card
          className="border-muted/50 w-full max-w-md shadow-lg"
          role="status"
          aria-live="polite"
          aria-busy="true"
        >
          <CardContent className="space-y-6 pt-6 pb-6">
            <div className="flex justify-center" aria-hidden="true">
              <Spinner className="size-8" />
            </div>
            <div className="space-y-2 text-center">
              <h1 className="text-foreground text-xl font-semibold">
                Authentication Required
              </h1>
              <p className="text-muted-foreground text-sm">
                Redirecting to secure login...
              </p>
            </div>
            <span className="sr-only">Redirecting to login page</span>
          </CardContent>
        </Card>
      </div>
    );
  }

  return <>{children}</>;
}
