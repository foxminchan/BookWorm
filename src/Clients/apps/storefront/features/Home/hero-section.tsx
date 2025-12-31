"use client";

import { Button } from "@workspace/ui/components/button";
import { useRouter } from "next/navigation";

export default function HeroSection() {
  const router = useRouter();

  return (
    <section
      className="py-20 md:py-32 container mx-auto px-4 text-center border-b"
      aria-labelledby="hero-heading"
    >
      <h1
        id="hero-heading"
        className="text-5xl md:text-7xl font-serif font-light mb-8 tracking-tight max-w-4xl mx-auto text-balance"
      >
        Great stories await your next chapter.
      </h1>
      <p className="text-lg md:text-xl text-muted-foreground mb-12 max-w-2xl mx-auto">
        Discover a curated collection of literature, design, and inspiration for
        the modern intellect.
      </p>
      <div className="flex flex-col sm:flex-row gap-4 justify-center">
        <Button
          size="lg"
          className="rounded-full px-8"
          onClick={() => router.push("/shop")}
        >
          Browse Collection
        </Button>
      </div>
    </section>
  );
}
