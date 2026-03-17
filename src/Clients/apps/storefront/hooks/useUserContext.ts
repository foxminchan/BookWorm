"use client";

import { useCopilotReadable } from "@copilotkit/react-core";

import { useSession } from "@/lib/auth-client";

export function useUserContext() {
  const { data: session } = useSession();

  const userData = {
    isAuthenticated: !!session?.user,
  };

  useCopilotReadable({
    description: "Whether the user is currently authenticated",
    value: userData,
  });
}
