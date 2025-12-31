"use client";

import { CategoryCard } from "./category-card";
import { CategoryCardSkeleton } from "@/components/loading-skeleton";

type Category = {
  id: string;
  name: string | null;
};

type CategoriesGridProps = {
  categories: Category[];
  isLoading: boolean;
};

export function CategoriesGrid({ categories, isLoading }: CategoriesGridProps) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 gap-px bg-border/40 border border-border/40">
        {Array.from({ length: 8 }).map((_, i) => (
          <CategoryCardSkeleton key={i} />
        ))}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-px bg-border/40 border border-border/40">
      {categories.map((category) => (
        <CategoryCard
          key={category.id}
          id={category.id}
          name={category.name || "Unknown Category"}
        />
      ))}
    </div>
  );
}
