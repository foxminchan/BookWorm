"use client";

import { useEffect } from "react";

import { Card, CardContent } from "@workspace/ui/components/card";
import { Spinner } from "@workspace/ui/components/spinner";

import { useUserContext } from "@/hooks/use-user-context";
import { signIn } from "@/lib/auth-client";
import { AUTH } from "@/lib/constants";

type AuthStatusCardProps = Readonly<{
  title: string;
  description: string;
}>;

function AuthStatusCard({ title, description }: AuthStatusCardProps) {
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
            <h1 className="text-foreground text-xl font-semibold">{title}</h1>
            <p className="text-muted-foreground text-sm">{description}</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export function AuthGuard({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  const { isAuthenticated, isLoading } = useUserContext();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      signIn.social({
        provider: AUTH.PROVIDER,
        callbackURL: AUTH.CALLBACK_URL,
      });
    }
  }, [isAuthenticated, isLoading]);

  if (isLoading) {
    return (
      <AuthStatusCard
        title="Loading Dashboard"
        description="Please wait while we verify your credentials..."
      />
    );
  }

  if (!isAuthenticated) {
    return (
      <AuthStatusCard
        title="Authentication Required"
        description="Redirecting to secure login..."
      />
    );
  }

  return children;
}
