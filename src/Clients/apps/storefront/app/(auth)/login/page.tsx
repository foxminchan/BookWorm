"use client";

import { useEffect } from "react";

import { Loader2 } from "lucide-react";

import { AUTH } from "@workspace/utils/constants";

import { signIn } from "@/lib/auth-client";

export default function LoginPage() {
  useEffect(() => {
    void signIn
      .social({
        provider: AUTH.PROVIDER,
        callbackURL: AUTH.CALLBACK_URL,
      })
      .catch((error: unknown) => {
        console.error("Failed to start Microsoft Entra ID login:", error);
      });
  }, []);

  return (
    <div className="space-y-4 text-center">
      <Loader2 className="text-primary mx-auto size-12 animate-spin" />
      <h1 className="text-2xl font-semibold">Redirecting to login...</h1>
      <p className="text-muted-foreground">
        Please wait while we redirect you to the login page.
      </p>
    </div>
  );
}
