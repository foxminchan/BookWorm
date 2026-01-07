"use client";

import { useCopilotReadable } from "@copilotkit/react-core";

import { useSession } from "@/lib/auth-client";

export function useUserContext() {
  const { data: session } = useSession();

  const userData = {
    isAuthenticated: !!session?.user,
    userId: session?.user?.id,
    name: session?.user?.name,
    email: session?.user?.email,
  };

  useCopilotReadable({
    description: "Information about the current authenticated user",
    value: userData,
  });
}
