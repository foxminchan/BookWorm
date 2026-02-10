"use client";

import { useRouter } from "next/navigation";

import { useShopData } from "@/hooks/useShopData";
import { useShopFilters } from "@/hooks/useShopFilters";

import BookGrid from "./book-grid";
import ShopFilters from "./shop-filters";
import ShopToolbar from "./shop-toolbar";

const ITEMS_PER_PAGE = 8;

export default function ShopPageClient() {
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

  const buildFilterUrl = (cats: string[], pubs: string[], auths: string[]) => {
    const params = new URLSearchParams();
    if (cats[0]) params.set("category", cats[0]);
    if (pubs[0]) params.set("publisher", pubs[0]);
    if (auths[0]) params.set("author", auths[0]);
    params.set("page", "1");
    return `/shop?${params.toString()}`;
  };

  const handleToggleCategory = (categoryId: string) => {
    const updated = selectedCategories.includes(categoryId)
      ? selectedCategories.filter((id) => id !== categoryId)
      : [...selectedCategories, categoryId];
    setSelectedCategories(updated);
    router.push(buildFilterUrl(updated, selectedPublishers, selectedAuthors));
  };

  const handleTogglePublisher = (publisherId: string) => {
    const updated = selectedPublishers.includes(publisherId)
      ? selectedPublishers.filter((id) => id !== publisherId)
      : [...selectedPublishers, publisherId];
    setSelectedPublishers(updated);
    router.push(buildFilterUrl(selectedCategories, updated, selectedAuthors));
  };

  const handleToggleAuthor = (authorId: string) => {
    const updated = selectedAuthors.includes(authorId)
      ? selectedAuthors.filter((id) => id !== authorId)
      : [...selectedAuthors, authorId];
    setSelectedAuthors(updated);
    router.push(
      buildFilterUrl(selectedCategories, selectedPublishers, updated),
    );
  };

  const handleClearSearch = () => {
    router.push(
      buildFilterUrl(selectedCategories, selectedPublishers, selectedAuthors),
    );
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
    <main className="container mx-auto grow px-4 py-8">
      <h1 className="sr-only">Shop Books - Browse Our Collection</h1>
      <div className="flex flex-col gap-8 md:flex-row">
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
            itemsPerPage={ITEMS_PER_PAGE}
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
  );
}
