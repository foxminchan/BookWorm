import Link from "next/link";

import { BookOpen, Home } from "lucide-react";

import { Button } from "@workspace/ui/components/button";

export default function NotFound() {
  return (
    <div className="bg-background flex min-h-screen items-center justify-center px-4">
      <div className="w-full max-w-2xl space-y-8 text-center">
        <div className="space-y-4">
          <h1 className="text-muted-foreground/20 font-serif text-9xl font-bold">
            404
          </h1>
          <h2 className="text-foreground font-serif text-4xl font-medium md:text-5xl">
            Page Not Found
          </h2>
          <p className="text-muted-foreground mx-auto max-w-md text-lg leading-relaxed">
            It seems the page you're looking for has wandered off into the
            stacks. Let's help you find your way back.
          </p>
        </div>
        <div className="flex flex-col items-center justify-center gap-4 pt-6 sm:flex-row">
          <Button
            asChild
            size="lg"
            className="shadow-primary/20 gap-2 rounded-full px-8 shadow-lg"
          >
            <Link href="/">
              <Home className="size-5" />
              Return Home
            </Link>
          </Button>
          <Button
            asChild
            variant="outline"
            size="lg"
            className="gap-2 rounded-full bg-transparent px-8"
          >
            <Link href="/shop">
              <BookOpen className="size-5" />
              Browse Books
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
}
