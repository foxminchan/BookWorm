"use client";

import { useAgentContext } from "@copilotkit/react-core/v2";

import { useUserSession } from "@workspace/auth/hooks";

import { useSession } from "@/lib/auth-client";

export function useUserContext() {
  const { isAuthenticated } = useUserSession(useSession);

  useAgentContext({
    description: "Whether the user is currently authenticated",
    value: {
      isAuthenticated,
    },
  });
}
