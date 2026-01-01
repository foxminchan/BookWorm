"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { EmptyState } from "@/components/empty-state";
import { FolderOpen } from "lucide-react";
import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";
import CategoriesGrid from "@/features/catalog/categories/categories-grid";
import CategoriesHeader from "@/features/catalog/categories/categories-header";

export default function CategoriesPage() {
  const { data: categories, isLoading } = useCategories();
  const categoryItems = categories ?? [];

  return (
    <div className="min-h-screen flex flex-col bg-background">
      <Header />

      <main className="grow container mx-auto px-4 py-16 md:py-24">
        <CategoriesHeader />

        {!isLoading && categoryItems.length === 0 ? (
          <EmptyState
            icon={FolderOpen}
            title="Coming Soon"
            description="We're setting up our collection. In the meantime, explore our shop for all available books."
            actionLabel="Browse the Shop"
            actionHref="/shop"
          />
        ) : (
          <CategoriesGrid categories={categoryItems} isLoading={isLoading} />
        )}
      </main>

      <Footer />
    </div>
  );
}
