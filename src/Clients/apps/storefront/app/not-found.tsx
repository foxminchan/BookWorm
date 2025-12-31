import Link from "next/link";
import { Button } from "@workspace/ui/components/button";
import { Home, BookOpen } from "lucide-react";

export default function NotFound() {
  return (
    <div className="min-h-screen bg-background flex items-center justify-center px-4">
      <div className="max-w-2xl w-full text-center space-y-8">
        <div className="space-y-4">
          <h1 className="text-9xl font-serif font-bold text-muted-foreground/20">
            404
          </h1>
          <h2 className="text-4xl md:text-5xl font-serif font-medium text-foreground">
            Page Not Found
          </h2>
          <p className="text-lg text-muted-foreground leading-relaxed max-w-md mx-auto">
            It seems the page you're looking for has wandered off into the
            stacks. Let's help you find your way back.
          </p>
        </div>
        <div className="flex flex-col sm:flex-row gap-4 justify-center items-center pt-6">
          <Button
            asChild
            size="lg"
            className="rounded-full gap-2 px-8 shadow-lg shadow-primary/20"
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
            className="rounded-full gap-2 px-8 bg-transparent"
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
