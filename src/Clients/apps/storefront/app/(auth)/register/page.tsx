"use client";

import { useEffect } from "react";

import { Loader2 } from "lucide-react";

import { signIn } from "@/lib/auth-client";

export default function RegisterPage() {
  useEffect(() => {
    void signIn
      .social({
        provider: "keycloak",
        callbackURL: "/",
      })
      .catch((error: unknown) => {
        console.error("Failed to start Keycloak registration:", error);
      });
  }, []);

  return (
    <div className="space-y-4 text-center">
      <Loader2 className="text-primary mx-auto size-12 animate-spin" />
      <h1 className="text-2xl font-semibold">Redirecting to registration...</h1>
      <p className="text-muted-foreground">
        Please wait while we redirect you to the registration page.
      </p>
    </div>
  );
}
