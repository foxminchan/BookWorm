"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import Link from "next/link";
import { ArrowRight } from "lucide-react";
import { CategoryCardSkeleton } from "@/components/category-card-skeleton";
import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";

export default function CategoriesPage() {
  const { data: categories, isLoading } = useCategories();
  const categoryItems = categories ?? [];

  return (
    <div className="min-h-screen flex flex-col bg-background">
      <Header />

      <main className="grow container mx-auto px-4 py-16 md:py-24">
        <div className="max-w-2xl mb-16">
          <h1 className="text-5xl md:text-6xl font-serif font-medium mb-6 tracking-tight">
            Genres
          </h1>
          <p className="text-muted-foreground text-lg leading-relaxed">
            From timeless classics to modern discoveries, find your next
            favorite read through our curated categories.
          </p>
        </div>

        {isLoading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-px bg-border/40 border border-border/40">
            {Array.from({ length: 8 }).map((_, i) => (
              <CategoryCardSkeleton key={i} />
            ))}
          </div>
        ) : categoryItems.length === 0 ? (
          <div className="text-center max-w-md">
            <p className="text-lg text-muted-foreground mb-8 leading-relaxed">
              We're setting up our collection. In the meantime, explore our shop
              for all available books.
            </p>
            <Link
              href="/shop"
              className="inline-flex items-center gap-2 text-primary hover:underline"
            >
              Browse the Shop <ArrowRight className="size-4" />
            </Link>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-px bg-border/40 border border-border/40">
            {categoryItems.map((category) => (
              <Link
                key={category.id}
                href={`/shop?category=${category.id}`}
                className="group relative flex items-center justify-between p-8 md:p-12 bg-background hover:bg-secondary/20 transition-all duration-300"
              >
                <div className="flex flex-col gap-1">
                  <span className="text-xs font-mono text-muted-foreground tracking-widest uppercase">
                    Genre
                  </span>
                  <h2 className="text-3xl md:text-4xl font-serif font-medium group-hover:translate-x-2 transition-transform duration-500">
                    {category.name}
                  </h2>
                </div>

                <div className="relative size-12 flex items-center justify-center border border-border group-hover:bg-primary group-hover:border-primary transition-all duration-500 rounded-full">
                  <ArrowRight className="size-5 group-hover:text-primary-foreground group-hover:translate-x-1 transition-all" />
                </div>
              </Link>
            ))}
          </div>
        )}
      </main>

      <Footer />
    </div>
  );
}
