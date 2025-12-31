"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import Link from "next/link";
import { ArrowRight } from "lucide-react";
import { PublisherCardSkeleton } from "@/components/publisher-card-skeleton";
import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";

export default function PublishersPage() {
  const { data: publishers, isLoading } = usePublishers();
  const publisherItems = publishers ?? [];

  if (!isLoading && publisherItems.length === 0) {
    return (
      <div className="min-h-screen flex flex-col bg-background">
        <Header />
        <main className="grow container mx-auto px-4 py-16 md:py-24 flex items-center justify-center">
          <div className="text-center max-w-md">
            <h1 className="text-4xl md:text-5xl font-serif font-light mb-4 tracking-tight">
              Coming Soon
            </h1>
            <p className="text-lg text-muted-foreground mb-8 leading-relaxed">
              We're still discovering amazing publishers to bring to BookWorm.
              Check back soon for an exciting collection.
            </p>
            <Link
              href="/"
              className="inline-flex items-center gap-2 text-primary hover:underline"
            >
              Return to Home <ArrowRight className="size-4" />
            </Link>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col bg-background">
      <Header />

      <main className="grow container mx-auto px-4 py-16 md:py-24">
        <div className="max-w-3xl mb-20">
          <h1 className="text-6xl md:text-7xl font-serif font-light mb-8 tracking-tight text-balance">
            Discover Premier Publishing Houses
          </h1>
          <p className="text-lg text-muted-foreground leading-relaxed max-w-xl">
            Explore the world's most respected publishers. Each represents a
            unique editorial voice, tradition, and commitment to bringing
            stories to readers.
          </p>
        </div>

        {isLoading ? (
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6 mb-20">
            {Array.from({ length: 9 }).map((_, i) => (
              <PublisherCardSkeleton key={i} />
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6 mb-20">
            {publisherItems.map((publisher) => (
              <Link
                key={publisher.id}
                href={`/shop?publisher=${publisher.id}`}
                className="group relative aspect-square rounded-lg overflow-hidden bg-linear-to-br from-secondary/10 to-secondary/5 border border-border/40 transition-all duration-500 hover:border-primary/40 hover:shadow-lg p-8 flex flex-col justify-between"
              >
                <div className="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-500 bg-linear-to-br from-primary/5 to-transparent" />

                <div className="relative z-10">
                  <div className="inline-block mb-4 px-3 py-1 rounded-full bg-primary/10 border border-primary/20">
                    <span className="text-xs font-mono text-primary uppercase tracking-widest">
                      Publisher
                    </span>
                  </div>
                </div>

                <div className="relative z-10 space-y-4">
                  <h2 className="text-2xl md:text-3xl font-serif font-medium group-hover:translate-x-1 transition-transform duration-500">
                    {publisher.name}
                  </h2>

                  <div className="inline-flex items-center gap-2 text-sm text-muted-foreground group-hover:text-primary transition-colors">
                    <span>Explore</span>
                    <ArrowRight className="size-4 group-hover:translate-x-2 transition-transform duration-500" />
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-12 border-t border-border/40 pt-16">
          <div>
            <h3 className="text-2xl font-serif mb-4">Why Publishers Matter</h3>
            <p className="text-muted-foreground leading-relaxed">
              Publishing houses are more than distributors—they're curators of
              human expression. Each brings distinctive editorial vision and
              commitment to authors and readers alike.
            </p>
          </div>
          <div>
            <h3 className="text-2xl font-serif mb-4">Find Your Next Read</h3>
            <p className="text-muted-foreground leading-relaxed">
              Browse books from your favorite publishers. Discover how each
              house shapes the literary landscape through their unique
              selections and commitments.
            </p>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
