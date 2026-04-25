"use client";

import { useAgentContext } from "@copilotkit/react-core/v2";

import { useSession } from "@/lib/auth-client";

export function useUserContext() {
  const { data: session } = useSession();

  const userData = {
    isAuthenticated: !!session?.user,
  };

  useAgentContext({
    description: "Whether the user is currently authenticated",
    value: userData,
  });
}
