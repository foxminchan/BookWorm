"use client";

import { useEffect, useState } from "react";

import { LogOut, Moon, Sun, User } from "lucide-react";
import { useTheme } from "next-themes";

import { Button } from "@workspace/ui/components/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@workspace/ui/components/dropdown-menu";

import { useLogout } from "@/hooks/use-logout";
import { useUserContext } from "@/hooks/use-user-context";

export function DashboardHeader() {
  const { theme, setTheme } = useTheme();
  const [mounted, setMounted] = useState(false);
  const { user } = useUserContext();
  const { logout } = useLogout();

  useEffect(() => {
    setMounted(true);
  }, []);

  const toggleTheme = () => setTheme(theme === "dark" ? "light" : "dark");

  const targetMode = theme === "dark" ? "light" : "dark";
  const themeLabel = mounted ? `Switch to ${targetMode} mode` : "Toggle theme";

  return (
    <header className="border-border bg-background flex items-center justify-between border-b px-6 py-4">
      <div>
        <h2 className="text-foreground text-2xl font-bold">Admin Portal</h2>
        <p className="text-muted-foreground text-sm">
          Welcome back to your dashboard
        </p>
      </div>

      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="icon"
          onClick={toggleTheme}
          aria-label={themeLabel}
          suppressHydrationWarning
        >
          <Sun
            className="h-5 w-5 scale-100 rotate-0 transition-all dark:scale-0 dark:-rotate-90"
            aria-hidden="true"
          />
          <Moon
            className="absolute h-5 w-5 scale-0 rotate-90 transition-all dark:scale-100 dark:rotate-0"
            aria-hidden="true"
          />
        </Button>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" aria-label="User menu">
              <User className="h-5 w-5" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuLabel>
              <div className="flex flex-col space-y-1">
                <p className="text-sm leading-none font-medium">
                  {user?.name ?? "Admin User"}
                </p>
                <p className="text-muted-foreground text-xs leading-none">
                  {user?.email ?? ""}
                </p>
              </div>
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={logout}>
              <LogOut className="mr-2 h-4 w-4" />
              <span>Log out</span>
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </header>
  );
}
