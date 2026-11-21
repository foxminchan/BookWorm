"use client";

import { Suspense, useEffect } from "react";

import { useSearchParams } from "next/navigation";

import { Loader2 } from "lucide-react";

import { AUTH } from "@workspace/utils/constants";

import { signIn } from "@/lib/auth-client";

type SocialAuthRedirectProps = Readonly<{
  title: string;
  description: string;
  errorContext: string;
  readCallbackFromQuery?: boolean;
}>;

function getSafeCallbackUrl(raw: string | null): string {
  if (raw && raw.startsWith("/") && !raw.startsWith("//")) {
    return raw;
  }
  return AUTH.CALLBACK_URL;
}

function RedirectTrigger({
  callbackURL,
  title,
  description,
  errorContext,
}: {
  callbackURL: string;
  title: string;
  description: string;
  errorContext: string;
}) {
  useEffect(() => {
    void signIn
      .social({
        provider: AUTH.PROVIDER,
        callbackURL,
      })
      .catch((error: unknown) => {
        console.error(errorContext, error);
      });
  }, [callbackURL, errorContext]);

  return (
    <div className="space-y-4 text-center">
      <Loader2
        className="text-primary mx-auto size-12 animate-spin"
        aria-hidden="true"
      />
      <h1 className="text-2xl font-semibold">{title}</h1>
      <p className="text-muted-foreground">{description}</p>
    </div>
  );
}

function QueryRedirectContent(
  props: Omit<SocialAuthRedirectProps, "readCallbackFromQuery">,
) {
  const searchParams = useSearchParams();
  const callbackURL = getSafeCallbackUrl(searchParams.get("callbackUrl"));

  return <RedirectTrigger {...props} callbackURL={callbackURL} />;
}

/**
 * Initiates Microsoft Entra ID OAuth login/registration redirect with a loading fallback.
 */
export function SocialAuthRedirect({
  title,
  description,
  errorContext,
  readCallbackFromQuery = false,
}: SocialAuthRedirectProps) {
  if (!readCallbackFromQuery) {
    return (
      <RedirectTrigger
        callbackURL={AUTH.CALLBACK_URL}
        title={title}
        description={description}
        errorContext={errorContext}
      />
    );
  }

  return (
    <Suspense
      fallback={
        <div className="space-y-4 text-center">
          <Loader2
            className="text-primary mx-auto size-12 animate-spin"
            aria-hidden="true"
          />
          <h1 className="text-2xl font-semibold">{title}</h1>
        </div>
      }
    >
      <QueryRedirectContent
        title={title}
        description={description}
        errorContext={errorContext}
      />
    </Suspense>
  );
}
