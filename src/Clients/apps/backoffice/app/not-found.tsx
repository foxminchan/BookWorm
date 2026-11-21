import Link from "next/link";

import { Button } from "@workspace/ui/components/button";

export default function NotFound() {
  return (
    <div className="bg-background flex min-h-screen items-center justify-center px-4">
      <div className="space-y-6 text-center">
        <div className="space-y-2">
          <h1 className="text-foreground text-6xl font-bold">404</h1>
          <h2 className="text-foreground/80 text-2xl font-semibold">
            Page Not Found
          </h2>
          <p className="text-foreground/60 max-w-md">
            The page you&apos;re looking for doesn&apos;t exist or has been
            moved. Let&apos;s get you back on track.
          </p>
        </div>

        <div className="flex justify-center gap-3">
          <Button
            render={<Link href="/">Go to Dashboard</Link>}
            nativeButton={false}
          />
          <Button
            render={<Link href="/books">Browse Books</Link>}
            nativeButton={false}
            variant="outline"
          />
        </div>
      </div>
    </div>
  );
}
