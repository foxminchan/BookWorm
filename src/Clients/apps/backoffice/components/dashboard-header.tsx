"use client";

export function DashboardHeader() {
  return (
    <header className="border-border bg-background flex items-center justify-between border-b px-6 py-4">
      <div>
        <h1 className="text-foreground text-2xl font-bold">Admin Portal</h1>
        <p className="text-muted-foreground text-sm">
          Welcome back to your dashboard
        </p>
      </div>
    </header>
  );
}
