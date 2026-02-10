"use client";

import Link from "next/link";

import { ArrowRight } from "lucide-react";

type CategoryCardProps = {
  id: string;
  name: string;
};

export default function CategoryCard({ id, name }: Readonly<CategoryCardProps>) {
  return (
    <Link
      href={`/shop?category=${id}`}
      className="group bg-background hover:bg-secondary/20 relative flex items-center justify-between p-8 transition-all duration-300 md:p-12"
    >
      <div className="flex flex-col gap-1">
        <span className="text-muted-foreground font-mono text-xs tracking-widest uppercase">
          Genre
        </span>
        <h2 className="font-serif text-3xl font-medium transition-transform duration-500 group-hover:translate-x-2 md:text-4xl">
          {name}
        </h2>
      </div>

      <div className="border-border group-hover:bg-primary group-hover:border-primary relative flex size-12 items-center justify-center rounded-full border transition-all duration-500">
        <ArrowRight className="group-hover:text-primary-foreground size-5 transition-all group-hover:translate-x-1" />
      </div>
    </Link>
  );
}
