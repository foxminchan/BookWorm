"use client";

import { CategoryCardSkeleton } from "@/components/loading-skeleton";

import CategoryCard from "./category-card";

type Category = {
  id: string;
  name: string | null;
};

type CategoriesGridProps = {
  categories: Category[];
  isLoading: boolean;
};

export default function CategoriesGrid({
  categories,
  isLoading,
}: CategoriesGridProps) {
  if (isLoading) {
    return (
      <div className="bg-border/40 border-border/40 grid grid-cols-1 gap-px border md:grid-cols-2">
        {Array.from({ length: 8 }).map((_, i) => (
          <CategoryCardSkeleton key={i} />
        ))}
      </div>
    );
  }

  return (
    <div className="bg-border/40 border-border/40 grid grid-cols-1 gap-px border md:grid-cols-2">
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
