import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { SocialAuthRedirect } from "@/features/auth/social-auth-redirect";

export const metadata: Metadata = createMetadata({
  title: "Login",
  description: "Sign in to your BookWorm account.",
});

export default function LoginPage() {
  return (
    <SocialAuthRedirect
      title="Redirecting to login..."
      description="Please wait while we redirect you to the login page."
      errorContext="Failed to start Microsoft Entra ID login:"
      readCallbackFromQuery
    />
  );
}
