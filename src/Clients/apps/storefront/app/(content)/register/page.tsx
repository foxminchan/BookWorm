"use client";

import { useEffect } from "react";

import { Loader2 } from "lucide-react";

import { signIn } from "@/lib/auth-client";

export default function RegisterPage() {
  useEffect(() => {
    const register = async () => {
      await signIn.oauth2({
        providerId: "keycloak",
        callbackURL: "/",
      });
    };

    register();
  }, []);

  return (
    <div className="container mx-auto flex min-h-[60vh] items-center justify-center">
      <div className="space-y-4 text-center">
        <Loader2 className="text-primary mx-auto size-12 animate-spin" />
        <h1 className="text-2xl font-semibold">
          Redirecting to registration...
        </h1>
        <p className="text-muted-foreground">
          Please wait while we redirect you to the registration page.
        </p>
      </div>
    </div>
  );
}
