"use client";

import type { Category } from "@workspace/types/catalog/categories";

type CategoriesSectionProps = {
  categories: Category[];
  isLoading: boolean;
};

export default function CategoriesSection({
  categories,
  isLoading,
}: CategoriesSectionProps) {
  const hasCategories = categories.length > 0;

  if (!hasCategories && !isLoading) {
    return (
      <section className="py-24 bg-secondary text-center">
        <div className="container mx-auto px-4">
          <p className="text-muted-foreground text-lg">
            Categories will be available soon
          </p>
        </div>
      </section>
    );
  }

  return (
    <section className="py-24 bg-secondary" aria-labelledby="category-heading">
      <div className="container mx-auto px-4">
        <h2
          id="category-heading"
          className="text-3xl font-serif font-medium mb-12 text-center"
        >
          Browse by Category
        </h2>
        <nav
          className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4"
          aria-label="Book Categories"
        >
          {isLoading
            ? Array.from({ length: 6 }).map((_, i) => (
                <div
                  key={i}
                  className="bg-background p-6 rounded-lg animate-pulse"
                >
                  <div className="h-5 bg-muted rounded" />
                </div>
              ))
            : categories.map((cat) => (
                <a
                  key={cat.id}
                  href={`/shop?category=${encodeURIComponent(cat.id)}`}
                  className="bg-background p-6 rounded-lg text-center hover:shadow-md transition-all hover:-translate-y-1 group"
                >
                  <h3 className="font-serif font-medium mb-1 group-hover:text-primary">
                    {cat.name}
                  </h3>
                </a>
              ))}
        </nav>
      </div>
    </section>
  );
}
