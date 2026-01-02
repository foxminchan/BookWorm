import { useState } from "react";

import { useRouter } from "next/navigation";

import { authClient } from "@/lib/auth-client";

type UseLogoutReturn = {
  logout: () => Promise<void>;
  isLoggingOut: boolean;
};

export function useLogout(): UseLogoutReturn {
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const router = useRouter();

  const logout = async () => {
    setIsLoggingOut(true);

    await authClient.signOut({
      fetchOptions: {
        onSuccess: () => {
          router.push("/");
        },
      },
    });
  };

  return { logout, isLoggingOut };
}
