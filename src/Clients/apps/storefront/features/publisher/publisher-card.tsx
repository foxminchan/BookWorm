"use client";

import Link from "next/link";
import { ArrowRight } from "lucide-react";

type PublisherCardProps = {
  id: string;
  name: string;
};

export function PublisherCard({ id, name }: PublisherCardProps) {
  return (
    <Link
      href={`/shop?publisher=${id}`}
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
          {name}
        </h2>

        <div className="inline-flex items-center gap-2 text-sm text-muted-foreground group-hover:text-primary transition-colors">
          <span>Explore</span>
          <ArrowRight className="size-4 group-hover:translate-x-2 transition-transform duration-500" />
        </div>
      </div>
    </Link>
  );
}
