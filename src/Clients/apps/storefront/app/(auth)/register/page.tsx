import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { SocialAuthRedirect } from "@/features/auth/social-auth-redirect";

export const metadata: Metadata = createMetadata({
  title: "Register",
  description: "Create your BookWorm account.",
});

export default function RegisterPage() {
  return (
    <SocialAuthRedirect
      title="Redirecting to registration..."
      description="Please wait while we redirect you to the registration page."
      errorContext="Failed to start Microsoft Entra ID registration:"
    />
  );
}
