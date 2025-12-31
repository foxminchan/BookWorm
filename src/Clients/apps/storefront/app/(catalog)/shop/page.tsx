"use client";

import { useRouter } from "next/navigation";
import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { BookGrid } from "@/features/shop/book-grid";
import { ShopFilters } from "@/features/shop/shop-filters";
import { ShopToolbar } from "@/features/shop/shop-toolbar";
import { useShopFilters } from "@/hooks/use-shop-filters";
import { useShopData } from "@/hooks/use-shop-data";

const ITEMS_PER_PAGE = 8;

export default function ShopPage() {
  const router = useRouter();
  const {
    currentPage,
    setCurrentPage,
    priceRange,
    setPriceRange,
    selectedAuthors,
    setSelectedAuthors,
    selectedPublishers,
    setSelectedPublishers,
    selectedCategories,
    setSelectedCategories,
    searchQuery,
    sortBy,
    setSortBy,
    isFilterOpen,
    setIsFilterOpen,
  } = useShopFilters();

  const { booksData, categories, publishers, authors, isLoading } = useShopData(
    {
      currentPage,
      priceRange,
      selectedCategories,
      selectedPublishers,
      selectedAuthors,
      searchQuery,
      sortBy,
    },
  );

  const updateFilters = (
    categories: string[],
    publishers: string[],
    authors: string[],
  ) => {
    const params = new URLSearchParams();
    if (categories.length > 0 && categories[0]) {
      params.set("category", categories[0]);
    }
    if (publishers.length > 0 && publishers[0]) {
      params.set("publisher", publishers[0]);
    }
    if (authors.length > 0 && authors[0]) {
      params.set("author", authors[0]);
    }
    params.set("page", "1");
    router.push(`/shop?${params.toString()}`);
  };

  const handleToggleCategory = (categoryId: string) => {
    const updated = selectedCategories.includes(categoryId)
      ? selectedCategories.filter((id: string) => id !== categoryId)
      : [...selectedCategories, categoryId];
    setSelectedCategories(updated);
    updateFilters(updated, selectedPublishers, selectedAuthors);
  };

  const handleTogglePublisher = (publisherId: string) => {
    const updated = selectedPublishers.includes(publisherId)
      ? selectedPublishers.filter((id: string) => id !== publisherId)
      : [...selectedPublishers, publisherId];
    setSelectedPublishers(updated);
    updateFilters(selectedCategories, updated, selectedAuthors);
  };

  const handleToggleAuthor = (authorId: string) => {
    const updated = selectedAuthors.includes(authorId)
      ? selectedAuthors.filter((id: string) => id !== authorId)
      : [...selectedAuthors, authorId];
    setSelectedAuthors(updated);
    updateFilters(selectedCategories, selectedPublishers, updated);
  };

  const handleClearSearch = () => {
    const params = new URLSearchParams();
    if (selectedCategories.length > 0 && selectedCategories[0]) {
      params.set("category", selectedCategories[0]);
    }
    if (selectedPublishers.length > 0 && selectedPublishers[0]) {
      params.set("publisher", selectedPublishers[0]);
    }
    if (selectedAuthors.length > 0 && selectedAuthors[0]) {
      params.set("author", selectedAuthors[0]);
    }
    params.set("page", "1");
    router.push(`/shop?${params.toString()}`);
  };

  const handleClearAllFilters = () => {
    setSelectedCategories([]);
    setSelectedPublishers([]);
    setPriceRange([0, 100]);
    setSelectedAuthors([]);
    setCurrentPage(1);
    router.push(`/shop?page=1`);
  };

  const books = Array.isArray(booksData?.items) ? booksData.items : [];
  const totalCount = booksData?.totalCount ?? 0;
  const totalPages = Math.ceil(totalCount / ITEMS_PER_PAGE);

  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="grow container mx-auto px-4 py-8">
        <div className="flex flex-col md:flex-row gap-8">
          <ShopFilters
            priceRange={priceRange}
            setPriceRange={setPriceRange}
            categories={categories}
            selectedCategories={selectedCategories}
            onToggleCategory={handleToggleCategory}
            publishers={publishers}
            selectedPublishers={selectedPublishers}
            onTogglePublisher={handleTogglePublisher}
            authors={authors}
            selectedAuthors={selectedAuthors}
            onToggleAuthor={handleToggleAuthor}
            isFilterOpen={isFilterOpen}
            setIsFilterOpen={setIsFilterOpen}
          />

          <section className="grow">
            <ShopToolbar
              searchQuery={searchQuery}
              onClearSearch={handleClearSearch}
              totalCount={totalCount}
              currentPage={currentPage}
              sortBy={sortBy}
              onSortChange={setSortBy}
              onOpenFilters={() => setIsFilterOpen(true)}
            />

            <BookGrid
              books={books}
              isLoading={isLoading}
              totalPages={totalPages}
              currentPage={currentPage}
              onPageChange={setCurrentPage}
              onClearFilters={handleClearAllFilters}
            />
          </section>
        </div>
      </main>
      <Footer />
    </div>
  );
}
