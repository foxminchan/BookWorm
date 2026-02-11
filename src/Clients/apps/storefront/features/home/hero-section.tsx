import Link from "next/link";

import { Button } from "@workspace/ui/components/button";

export default function HeroSection() {
  return (
    <section
      className="container mx-auto border-b px-4 py-20 text-center md:py-32"
      aria-labelledby="hero-heading"
    >
      <h1
        id="hero-heading"
        className="mx-auto mb-8 max-w-4xl font-serif text-5xl font-light tracking-tight text-balance md:text-7xl"
      >
        Great stories await your next chapter.
      </h1>
      <p className="text-muted-foreground mx-auto mb-12 max-w-2xl text-lg md:text-xl">
        Discover a curated collection of literature, design, and inspiration for
        the modern intellect.
      </p>
      <div className="flex flex-col justify-center gap-4 sm:flex-row">
        <Button
          size="lg"
          className="rounded-full px-8"
          aria-label="Browse book collection"
          asChild
        >
          <Link href="/shop">Browse Collection</Link>
        </Button>
      </div>
    </section>
  );
}
