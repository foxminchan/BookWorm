"use client";

import Link from "next/link";
import { ArrowRight } from "lucide-react";

type CategoryCardProps = {
  id: string;
  name: string;
};

export default function CategoryCard({ id, name }: CategoryCardProps) {
  return (
    <Link
      href={`/shop?category=${id}`}
      className="group relative flex items-center justify-between p-8 md:p-12 bg-background hover:bg-secondary/20 transition-all duration-300"
    >
      <div className="flex flex-col gap-1">
        <span className="text-xs font-mono text-muted-foreground tracking-widest uppercase">
          Genre
        </span>
        <h2 className="text-3xl md:text-4xl font-serif font-medium group-hover:translate-x-2 transition-transform duration-500">
          {name}
        </h2>
      </div>

      <div className="relative size-12 flex items-center justify-center border border-border group-hover:bg-primary group-hover:border-primary transition-all duration-500 rounded-full">
        <ArrowRight className="size-5 group-hover:text-primary-foreground group-hover:translate-x-1 transition-all" />
      </div>
    </Link>
  );
}
