"use client";

import { Loader2 } from "lucide-react";
import { Button } from "@workspace/ui/components/button";
import { useLogout } from "@/hooks/useLogout";

export default function LogoutButton() {
  const { logout, isLoggingOut } = useLogout();

  return (
    <Button
      onClick={logout}
      disabled={isLoggingOut}
      className="w-full border border-border/40 bg-transparent hover:bg-secondary/20 text-foreground justify-center gap-2 h-12 font-medium"
    >
      {isLoggingOut ? (
        <>
          <Loader2 className="size-4 animate-spin" />
          Logging out...
        </>
      ) : (
        "Logout"
      )}
    </Button>
  );
}
