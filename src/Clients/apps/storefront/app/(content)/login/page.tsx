"use client";

import { useEffect } from "react";
import { signIn } from "@/lib/auth-client";
import { Loader2 } from "lucide-react";

export default function LoginPage() {
  useEffect(() => {
    const login = async () => {
      await signIn.oauth2({
        providerId: "keycloak",
        callbackURL: "/",
      });
    };

    login();
  }, []);

  return (
    <div className="container mx-auto flex min-h-[60vh] items-center justify-center">
      <div className="text-center space-y-4">
        <Loader2 className="size-12 animate-spin mx-auto text-primary" />
        <h1 className="text-2xl font-semibold">Redirecting to login...</h1>
        <p className="text-muted-foreground">
          Please wait while we redirect you to the login page.
        </p>
      </div>
    </div>
  );
}
