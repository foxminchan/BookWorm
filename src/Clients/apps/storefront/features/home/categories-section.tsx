"use client";

import Link from "next/link";

import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";

export default function CategoriesSection() {
  // This will use the hydrated data from the server
  const { data: categoriesData, isLoading } = useCategories();

  const categories = Array.isArray(categoriesData)
    ? categoriesData.slice(0, 6)
    : [];
  const hasCategories = categories.length > 0;

  if (!hasCategories && !isLoading) {
    return (
      <section className="bg-secondary py-24 text-center">
        <div className="container mx-auto px-4">
          <p className="text-muted-foreground text-lg">
            Categories will be available soon
          </p>
        </div>
      </section>
    );
  }

  return (
    <section className="bg-secondary py-24" aria-labelledby="category-heading">
      <div className="container mx-auto px-4">
        <h2
          id="category-heading"
          className="mb-12 text-center font-serif text-3xl font-medium"
        >
          Browse by Category
        </h2>
        <nav
          className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-6"
          aria-label="Book Categories"
        >
          {isLoading
            ? Array.from({ length: 6 }).map((_, i) => (
                <div
                  key={`skeleton-${i.toString()}`}
                  className="bg-background animate-pulse rounded-lg p-6"
                >
                  <div className="bg-muted h-5 rounded" />
                </div>
              ))
            : categories.map((cat) => (
                <Link
                  key={cat.id}
                  href={`/shop?category=${encodeURIComponent(cat.id)}`}
                  className="bg-background group rounded-lg p-6 text-center transition-all hover:-translate-y-1 hover:shadow-md"
                >
                  <h3 className="group-hover:text-primary mb-1 font-serif font-medium">
                    {cat.name}
                  </h3>
                </Link>
              ))}
        </nav>
      </div>
    </section>
  );
}
