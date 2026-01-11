"use client";

import { useEffect, useState } from "react";

import { Moon, Sun } from "lucide-react";
import { useTheme } from "next-themes";

import { Button } from "@workspace/ui/components/button";

export function DashboardHeader() {
  const { theme, setTheme } = useTheme();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  return (
    <header className="border-border bg-background flex items-center justify-between border-b px-6 py-4">
      <div>
        <div className="text-foreground text-2xl font-bold">Admin Portal</div>
        <p className="text-muted-foreground text-sm">
          Welcome back to your dashboard
        </p>
      </div>

      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="icon"
          onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
          aria-label={
            mounted
              ? `Switch to ${theme === "dark" ? "light" : "dark"} mode`
              : "Toggle theme"
          }
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
          <span className="sr-only">Toggle theme</span>
        </Button>
      </div>
    </header>
  );
}
