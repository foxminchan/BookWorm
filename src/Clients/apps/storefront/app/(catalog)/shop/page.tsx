"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { Card, CardContent } from "@workspace/ui/components/card";
import { Button } from "@workspace/ui/components/button";
import { Slider } from "@workspace/ui/components/slider";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";
import { Input } from "@workspace/ui/components/input";
import { Search } from "lucide-react";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@workspace/ui/components/sheet";
import { Star, Filter, X } from "lucide-react";
import { useEffect, useState, useMemo } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useDebounceValue } from "usehooks-ts";
import { BookCardSkeleton } from "@/components/book-card-skeleton";
import { Badge } from "@workspace/ui/components/badge";
import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";
import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";
import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";

const ITEMS_PER_PAGE = 8;

type ShopContentProps = {
  booksData: any;
  categories: any[] | undefined;
  publishers: any[] | undefined;
  authors: any[] | undefined;
  searchQuery: string;
  priceRange: number[];
  setPriceRange: (range: number[]) => void;
  selectedAuthors: string[];
  setSelectedAuthors: React.Dispatch<React.SetStateAction<string[]>>;
  selectedPublishers: string[];
  setSelectedPublishers: React.Dispatch<React.SetStateAction<string[]>>;
  selectedCategories: string[];
  setSelectedCategories: React.Dispatch<React.SetStateAction<string[]>>;
  sortBy: string;
  setSortBy: (value: string) => void;
  isLoading: boolean;
  isFilterOpen: boolean;
  setIsFilterOpen: (value: boolean) => void;
  expandedSections: {
    categories: boolean;
    publishers: boolean;
    authors: boolean;
  };
  setExpandedSections: React.Dispatch<
    React.SetStateAction<{
      categories: boolean;
      publishers: boolean;
      authors: boolean;
    }>
  >;
  filterSearches: { categories: string; publishers: string; authors: string };
  setFilterSearches: React.Dispatch<
    React.SetStateAction<{
      categories: string;
      publishers: string;
      authors: string;
    }>
  >;
  searchOpen: { categories: boolean; publishers: boolean; authors: boolean };
  setSearchOpen: React.Dispatch<
    React.SetStateAction<{
      categories: boolean;
      publishers: boolean;
      authors: boolean;
    }>
  >;
  currentPage: number;
  setCurrentPage: (page: number) => void;
};

function ShopContent({
  booksData,
  categories,
  publishers,
  authors,
  searchQuery,
  priceRange,
  setPriceRange,
  selectedAuthors,
  setSelectedAuthors,
  selectedPublishers,
  setSelectedPublishers,
  selectedCategories,
  setSelectedCategories,
  sortBy,
  setSortBy,
  isLoading,
  isFilterOpen,
  setIsFilterOpen,
  expandedSections,
  setExpandedSections,
  filterSearches,
  setFilterSearches,
  searchOpen,
  setSearchOpen,
  currentPage,
  setCurrentPage,
}: ShopContentProps) {
  const router = useRouter();

  const clearSearch = () => {
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

  const toggleCategory = (categoryId: string) => {
    const updated = selectedCategories.includes(categoryId)
      ? selectedCategories.filter((id: string) => id !== categoryId)
      : [...selectedCategories, categoryId];
    setSelectedCategories(updated);
    updateFilters(updated, selectedPublishers, selectedAuthors);
  };

  const togglePublisher = (publisherId: string) => {
    const updated = selectedPublishers.includes(publisherId)
      ? selectedPublishers.filter((id: string) => id !== publisherId)
      : [...selectedPublishers, publisherId];
    setSelectedPublishers(updated);
    updateFilters(selectedCategories, updated, selectedAuthors);
  };

  const toggleAuthor = (authorId: string) => {
    const updated = selectedAuthors.includes(authorId)
      ? selectedAuthors.filter((id: string) => id !== authorId)
      : [...selectedAuthors, authorId];
    setSelectedAuthors(updated);
    updateFilters(selectedCategories, selectedPublishers, updated);
  };

  const clearAllFilters = () => {
    setSelectedCategories([]);
    setSelectedPublishers([]);
    setPriceRange([0, 100]);
    setSelectedAuthors([]);
    setCurrentPage(1);
    router.push(`/shop?page=1`);
  };

  // API handles filtering, sorting, and pagination
  const books = Array.isArray(booksData?.items) ? booksData.items : [];
  const totalCount = booksData?.totalCount ?? 0;
  const totalPages = Math.ceil(totalCount / ITEMS_PER_PAGE);

  const createPageLink = (pageNum: number) => {
    const params = new URLSearchParams();
    params.set("page", pageNum.toString());
    return `/shop?${params.toString()}`;
  };

  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxPages = 5;
    const halfWindow = Math.floor(maxPages / 2);

    if (totalPages <= maxPages) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (currentPage <= halfWindow) {
        for (let i = 1; i <= maxPages; i++) {
          pages.push(i);
        }
        pages.push("...");
        pages.push(totalPages);
      } else if (currentPage >= totalPages - halfWindow) {
        pages.push(1);
        pages.push("...");
        for (let i = totalPages - maxPages + 1; i <= totalPages; i++) {
          pages.push(i);
        }
      } else {
        pages.push(1);
        pages.push("...");
        for (let i = currentPage - 1; i <= currentPage + 1; i++) {
          pages.push(i);
        }
        pages.push("...");
        pages.push(totalPages);
      }
    }
    return pages;
  };

  const visibleCategories = useMemo(() => {
    return (categories || []).filter((category: any) =>
      category.name
        .toLowerCase()
        .includes(filterSearches.categories.toLowerCase()),
    );
  }, [categories, filterSearches.categories]);

  const visiblePublishers = useMemo(() => {
    return (publishers || []).filter((publisher: any) =>
      publisher.name
        .toLowerCase()
        .includes(filterSearches.publishers.toLowerCase()),
    );
  }, [publishers, filterSearches.publishers]);

  const visibleAuthors = useMemo(() => {
    return (authors || []).filter((author: any) =>
      author.name.toLowerCase().includes(filterSearches.authors.toLowerCase()),
    );
  }, [authors, filterSearches.authors]);

  return (
    <div className="flex flex-col md:flex-row gap-8">
      {/* Desktop Sidebar Filters - Hidden on Mobile */}
      <aside
        className="hidden md:block w-64 space-y-8 shrink-0"
        aria-label="Filters"
      >
        {/* Price Range */}
        <div>
          <h3 className="font-serif font-medium mb-4">Price Range</h3>
          <Slider
            value={priceRange}
            onValueChange={(value) =>
              setPriceRange([value[0] ?? 0, value[1] ?? 100])
            }
            max={100}
            step={1}
            className="mb-2"
          />
          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>${priceRange[0]}</span>
            <span>${priceRange[1]}+</span>
          </div>
        </div>

        {/* Categories */}
        <div>
          <div className="flex items-center justify-between gap-2 mb-4">
            <h3 className="font-serif font-medium">Category</h3>
            <button
              onClick={() =>
                setSearchOpen(
                  (prev: {
                    categories: boolean;
                    publishers: boolean;
                    authors: boolean;
                  }) => ({
                    ...prev,
                    categories: !prev.categories,
                  }),
                )
              }
              className="text-muted-foreground hover:text-foreground transition"
            >
              <Search className="size-4" />
            </button>
          </div>
          {searchOpen.categories && (
            <div className="relative mb-4">
              <Search className="absolute left-2 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
              <Input
                placeholder="Search categories..."
                className="pl-8 h-8 text-sm"
                value={filterSearches.categories}
                onChange={(e) =>
                  setFilterSearches(
                    (prev: {
                      categories: string;
                      publishers: string;
                      authors: string;
                    }) => ({
                      ...prev,
                      categories: e.target.value,
                    }),
                  )
                }
                autoFocus
              />
            </div>
          )}
          <div
            className={`space-y-2 overflow-hidden transition-all duration-300 ease-in-out ${
              expandedSections.categories ? "max-h-96" : "max-h-56"
            }`}
          >
            {visibleCategories
              .slice(0, expandedSections.categories ? undefined : 5)
              .map((category: any) => (
                <label
                  key={category.id}
                  className="flex items-center gap-2 cursor-pointer group"
                >
                  <div className="relative w-4 h-4">
                    <input
                      type="checkbox"
                      checked={selectedCategories.includes(category.id)}
                      onChange={() => toggleCategory(category.id)}
                      className="appearance-none w-4 h-4 border border-border rounded bg-card checked:bg-primary checked:border-primary transition cursor-pointer"
                    />
                    {selectedCategories.includes(category.id) && (
                      <svg
                        className="absolute inset-0 w-4 h-4 text-primary-foreground pointer-events-none"
                        viewBox="0 0 16 16"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                      >
                        <path d="M3 8l3 3 7-7" />
                      </svg>
                    )}
                  </div>
                  <span className="text-sm">{category.name}</span>
                </label>
              ))}
          </div>
          {visibleCategories.length > 5 && (
            <button
              onClick={() =>
                setExpandedSections(
                  (prev: {
                    categories: boolean;
                    publishers: boolean;
                    authors: boolean;
                  }) => ({
                    ...prev,
                    categories: !prev.categories,
                  }),
                )
              }
              className="text-sm text-primary hover:underline mt-2 transition-colors"
            >
              {expandedSections.categories ? "Show Less" : "Show More"}
            </button>
          )}
        </div>

        {/* Publishers */}
        <div>
          <div className="flex items-center justify-between gap-2 mb-4">
            <h3 className="font-serif font-medium">Publisher</h3>
            <button
              onClick={() =>
                setSearchOpen(
                  (prev: {
                    categories: boolean;
                    publishers: boolean;
                    authors: boolean;
                  }) => ({
                    ...prev,
                    publishers: !prev.publishers,
                  }),
                )
              }
              className="text-muted-foreground hover:text-foreground transition"
            >
              <Search className="size-4" />
            </button>
          </div>
          {searchOpen.publishers && (
            <div className="relative mb-4">
              <Search className="absolute left-2 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
              <Input
                placeholder="Search publishers..."
                className="pl-8 h-8 text-sm"
                value={filterSearches.publishers}
                onChange={(e) =>
                  setFilterSearches(
                    (prev: {
                      categories: string;
                      publishers: string;
                      authors: string;
                    }) => ({
                      ...prev,
                      publishers: e.target.value,
                    }),
                  )
                }
                autoFocus
              />
            </div>
          )}
          <div
            className={`space-y-2 overflow-hidden transition-all duration-300 ease-in-out ${
              expandedSections.publishers ? "max-h-96" : "max-h-56"
            }`}
          >
            {visiblePublishers
              .slice(0, expandedSections.publishers ? undefined : 5)
              .map((publisher: any) => (
                <label
                  key={publisher.id}
                  className="flex items-center gap-2 cursor-pointer"
                >
                  <div className="relative w-4 h-4">
                    <input
                      type="checkbox"
                      checked={selectedPublishers.includes(publisher.id)}
                      onChange={() => togglePublisher(publisher.id)}
                      className="appearance-none w-4 h-4 border border-border rounded bg-card checked:bg-primary checked:border-primary transition cursor-pointer"
                    />
                    {selectedPublishers.includes(publisher.id) && (
                      <svg
                        className="absolute inset-0 w-4 h-4 text-primary-foreground pointer-events-none"
                        viewBox="0 0 16 16"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                      >
                        <path d="M3 8l3 3 7-7" />
                      </svg>
                    )}
                  </div>
                  <span className="text-sm">{publisher.name}</span>
                </label>
              ))}
          </div>
          {visiblePublishers.length > 5 && (
            <button
              onClick={() =>
                setExpandedSections(
                  (prev: {
                    categories: boolean;
                    publishers: boolean;
                    authors: boolean;
                  }) => ({
                    ...prev,
                    publishers: !prev.publishers,
                  }),
                )
              }
              className="text-sm text-primary hover:underline mt-2 transition-colors"
            >
              {expandedSections.publishers ? "Show Less" : "Show More"}
            </button>
          )}
        </div>

        {/* Authors */}
        <div>
          <div className="flex items-center justify-between gap-2 mb-4">
            <h3 className="font-serif font-medium">Author</h3>
            <button
              onClick={() =>
                setSearchOpen(
                  (prev: {
                    categories: boolean;
                    publishers: boolean;
                    authors: boolean;
                  }) => ({ ...prev, authors: !prev.authors }),
                )
              }
              className="text-muted-foreground hover:text-foreground transition"
            >
              <Search className="size-4" />
            </button>
          </div>
          {searchOpen.authors && (
            <div className="relative mb-4">
              <Search className="absolute left-2 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
              <Input
                placeholder="Search authors..."
                className="pl-8 h-8 text-sm"
                value={filterSearches.authors}
                onChange={(e) =>
                  setFilterSearches(
                    (prev: {
                      categories: string;
                      publishers: string;
                      authors: string;
                    }) => ({
                      ...prev,
                      authors: e.target.value,
                    }),
                  )
                }
                autoFocus
              />
            </div>
          )}
          <div
            className={`space-y-2 overflow-hidden transition-all duration-300 ease-in-out ${
              expandedSections.authors ? "max-h-96" : "max-h-56"
            }`}
          >
            {visibleAuthors
              .slice(0, expandedSections.authors ? undefined : 5)
              .map((author: any) => (
                <label
                  key={author.id}
                  className="flex items-center gap-2 cursor-pointer"
                >
                  <div className="relative w-4 h-4">
                    <input
                      type="checkbox"
                      checked={selectedAuthors.includes(author.id)}
                      onChange={() => toggleAuthor(author.id)}
                      className="appearance-none w-4 h-4 border border-border rounded bg-card checked:bg-primary checked:border-primary transition cursor-pointer"
                    />
                    {selectedAuthors.includes(author.id) && (
                      <svg
                        className="absolute inset-0 w-4 h-4 text-primary-foreground pointer-events-none"
                        viewBox="0 0 16 16"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                      >
                        <path d="M3 8l3 3 7-7" />
                      </svg>
                    )}
                  </div>
                  <span className="text-sm">{author.name}</span>
                </label>
              ))}
          </div>
          {visibleAuthors.length > 5 && (
            <button
              onClick={() =>
                setExpandedSections(
                  (prev: {
                    categories: boolean;
                    publishers: boolean;
                    authors: boolean;
                  }) => ({
                    ...prev,
                    authors: !prev.authors,
                  }),
                )
              }
              className="text-sm text-primary hover:underline mt-2 transition-colors"
            >
              {expandedSections.authors ? "Show Less" : "Show More"}
            </button>
          )}
        </div>
      </aside>

      {/* Mobile Filter Sheet */}
      <Sheet open={isFilterOpen} onOpenChange={setIsFilterOpen}>
        <SheetContent side="left" className="overflow-y-auto">
          <SheetHeader>
            <SheetTitle>Filters</SheetTitle>
          </SheetHeader>

          {/* Price Range */}
          <div className="space-y-8 mt-8 px-4">
            <div>
              <h3 className="font-serif font-medium mb-4">Price Range</h3>
              <Slider
                value={priceRange}
                onValueChange={(value) =>
                  setPriceRange([value[0] ?? 0, value[1] ?? 100])
                }
                max={100}
                step={1}
                className="mb-2"
              />
              <div className="flex items-center justify-between text-sm text-muted-foreground">
                <span>${priceRange[0]}</span>
                <span>${priceRange[1]}+</span>
              </div>
            </div>

            {/* Categories */}
            <div>
              <div className="flex items-center justify-between gap-2 mb-4">
                <h3 className="font-serif font-medium">Category</h3>
                <button
                  onClick={() =>
                    setSearchOpen(
                      (prev: {
                        categories: boolean;
                        publishers: boolean;
                        authors: boolean;
                      }) => ({
                        ...prev,
                        categories: !prev.categories,
                      }),
                    )
                  }
                  className="text-muted-foreground hover:text-foreground transition"
                >
                  <Search className="size-4" />
                </button>
              </div>
              {searchOpen.categories && (
                <div className="relative mb-4">
                  <Search className="absolute left-2 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
                  <Input
                    placeholder="Search categories..."
                    className="pl-8 h-8 text-sm"
                    value={filterSearches.categories}
                    onChange={(e) =>
                      setFilterSearches(
                        (prev: {
                          categories: string;
                          publishers: string;
                          authors: string;
                        }) => ({
                          ...prev,
                          categories: e.target.value,
                        }),
                      )
                    }
                    autoFocus
                  />
                </div>
              )}
              <div
                className={`space-y-2 overflow-hidden transition-all duration-300 ease-in-out ${
                  expandedSections.categories ? "max-h-96" : "max-h-56"
                }`}
              >
                {visibleCategories
                  .slice(0, expandedSections.categories ? undefined : 5)
                  .map((category: any) => (
                    <label
                      key={category.id}
                      className="flex items-center gap-2 cursor-pointer group"
                    >
                      <div className="relative w-4 h-4">
                        <input
                          type="checkbox"
                          checked={selectedCategories.includes(category.id)}
                          onChange={() => toggleCategory(category.id)}
                          className="appearance-none w-4 h-4 border border-border rounded bg-card checked:bg-primary checked:border-primary transition cursor-pointer"
                        />
                        {selectedCategories.includes(category.id) && (
                          <svg
                            className="absolute inset-0 w-4 h-4 text-primary-foreground pointer-events-none"
                            viewBox="0 0 16 16"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                          >
                            <path d="M3 8l3 3 7-7" />
                          </svg>
                        )}
                      </div>
                      <span className="text-sm">{category.name}</span>
                    </label>
                  ))}
              </div>
              {visibleCategories.length > 5 && (
                <button
                  onClick={() =>
                    setExpandedSections(
                      (prev: {
                        categories: boolean;
                        publishers: boolean;
                        authors: boolean;
                      }) => ({
                        ...prev,
                        categories: !prev.categories,
                      }),
                    )
                  }
                  className="text-sm text-primary hover:underline mt-2 transition-colors"
                >
                  {expandedSections.categories ? "Show Less" : "Show More"}
                </button>
              )}
            </div>

            {/* Publishers */}
            <div>
              <div className="flex items-center justify-between gap-2 mb-4">
                <h3 className="font-serif font-medium">Publisher</h3>
                <button
                  onClick={() =>
                    setSearchOpen(
                      (prev: {
                        categories: boolean;
                        publishers: boolean;
                        authors: boolean;
                      }) => ({
                        ...prev,
                        publishers: !prev.publishers,
                      }),
                    )
                  }
                  className="text-muted-foreground hover:text-foreground transition"
                >
                  <Search className="size-4" />
                </button>
              </div>
              {searchOpen.publishers && (
                <div className="relative mb-4">
                  <Search className="absolute left-2 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
                  <Input
                    placeholder="Search publishers..."
                    className="pl-8 h-8 text-sm"
                    value={filterSearches.publishers}
                    onChange={(e) =>
                      setFilterSearches(
                        (prev: {
                          categories: string;
                          publishers: string;
                          authors: string;
                        }) => ({
                          ...prev,
                          publishers: e.target.value,
                        }),
                      )
                    }
                    autoFocus
                  />
                </div>
              )}
              <div
                className={`space-y-2 overflow-hidden transition-all duration-300 ease-in-out ${
                  expandedSections.publishers ? "max-h-96" : "max-h-56"
                }`}
              >
                {visiblePublishers
                  .slice(0, expandedSections.publishers ? undefined : 5)
                  .map((publisher: any) => (
                    <label
                      key={publisher.id}
                      className="flex items-center gap-2 cursor-pointer"
                    >
                      <div className="relative w-4 h-4">
                        <input
                          type="checkbox"
                          checked={selectedPublishers.includes(publisher.id)}
                          onChange={() => togglePublisher(publisher.id)}
                          className="appearance-none w-4 h-4 border border-border rounded bg-card checked:bg-primary checked:border-primary transition cursor-pointer"
                        />
                        {selectedPublishers.includes(publisher.id) && (
                          <svg
                            className="absolute inset-0 w-4 h-4 text-primary-foreground pointer-events-none"
                            viewBox="0 0 16 16"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                          >
                            <path d="M3 8l3 3 7-7" />
                          </svg>
                        )}
                      </div>
                      <span className="text-sm">{publisher.name}</span>
                    </label>
                  ))}
              </div>
              {visiblePublishers.length > 5 && (
                <button
                  onClick={() =>
                    setExpandedSections(
                      (prev: {
                        categories: boolean;
                        publishers: boolean;
                        authors: boolean;
                      }) => ({
                        ...prev,
                        publishers: !prev.publishers,
                      }),
                    )
                  }
                  className="text-sm text-primary hover:underline mt-2 transition-colors"
                >
                  {expandedSections.publishers ? "Show Less" : "Show More"}
                </button>
              )}
            </div>

            {/* Authors */}
            <div>
              <div className="flex items-center justify-between gap-2 mb-4">
                <h3 className="font-serif font-medium">Author</h3>
                <button
                  onClick={() =>
                    setSearchOpen(
                      (prev: {
                        categories: boolean;
                        publishers: boolean;
                        authors: boolean;
                      }) => ({
                        ...prev,
                        authors: !prev.authors,
                      }),
                    )
                  }
                  className="text-muted-foreground hover:text-foreground transition"
                >
                  <Search className="size-4" />
                </button>
              </div>
              {searchOpen.authors && (
                <div className="relative mb-4">
                  <Search className="absolute left-2 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
                  <Input
                    placeholder="Search authors..."
                    className="pl-8 h-8 text-sm"
                    value={filterSearches.authors}
                    onChange={(e) =>
                      setFilterSearches(
                        (prev: {
                          categories: string;
                          publishers: string;
                          authors: string;
                        }) => ({
                          ...prev,
                          authors: e.target.value,
                        }),
                      )
                    }
                    autoFocus
                  />
                </div>
              )}
              <div
                className={`space-y-2 overflow-hidden transition-all duration-300 ease-in-out ${
                  expandedSections.authors ? "max-h-96" : "max-h-56"
                }`}
              >
                {visibleAuthors
                  .slice(0, expandedSections.authors ? undefined : 5)
                  .map((author: any) => (
                    <label
                      key={author.id}
                      className="flex items-center gap-2 cursor-pointer"
                    >
                      <div className="relative w-4 h-4">
                        <input
                          type="checkbox"
                          checked={selectedAuthors.includes(author.id)}
                          onChange={() => toggleAuthor(author.id)}
                          className="appearance-none w-4 h-4 border border-border rounded bg-card checked:bg-primary checked:border-primary transition cursor-pointer"
                        />
                        {selectedAuthors.includes(author.id) && (
                          <svg
                            className="absolute inset-0 w-4 h-4 text-primary-foreground pointer-events-none"
                            viewBox="0 0 16 16"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="2"
                          >
                            <path d="M3 8l3 3 7-7" />
                          </svg>
                        )}
                      </div>
                      <span className="text-sm">{author.name}</span>
                    </label>
                  ))}
              </div>
              {visibleAuthors.length > 5 && (
                <button
                  onClick={() =>
                    setExpandedSections(
                      (prev: {
                        categories: boolean;
                        publishers: boolean;
                        authors: boolean;
                      }) => ({
                        ...prev,
                        authors: !prev.authors,
                      }),
                    )
                  }
                  className="text-sm text-primary hover:underline mt-2 transition-colors"
                >
                  {expandedSections.authors ? "Show Less" : "Show More"}
                </button>
              )}
            </div>
          </div>
        </SheetContent>
      </Sheet>

      {/* Main Content */}
      <section className="grow">
        {/* Shop Header / Toolbar */}
        <div
          className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8"
          role="region"
          aria-labelledby="shop-title"
        >
          <div>
            <h1 id="shop-title" className="text-3xl font-serif font-medium">
              Shop All Books
            </h1>
            {searchQuery ? (
              <div className="flex items-center gap-2 mt-2">
                <p className="text-sm text-muted-foreground">
                  Search results for
                </p>
                <Badge
                  variant="secondary"
                  className="font-normal flex items-center gap-1.5"
                >
                  "{searchQuery}"
                  <button
                    onClick={clearSearch}
                    className="ml-1 hover:bg-secondary-foreground/10 rounded-full p-0.5 transition-colors"
                    aria-label="Clear search"
                  >
                    <X className="size-3" />
                  </button>
                </Badge>
                <p className="text-sm text-muted-foreground">
                  ({totalCount} {totalCount === 1 ? "result" : "results"})
                </p>
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">
                Showing {(currentPage - 1) * ITEMS_PER_PAGE + 1}–
                {Math.min(currentPage * ITEMS_PER_PAGE, totalCount)} of{" "}
                {totalCount} results
              </p>
            )}
          </div>

          <div className="flex items-center gap-4 w-full sm:w-auto">
            <Button
              variant="outline"
              size="sm"
              className="md:hidden flex items-center gap-2 bg-transparent"
              onClick={() => setIsFilterOpen(true)}
              aria-label="Open filters"
            >
              <Filter className="size-4" /> Filters
            </Button>
            <Select value={sortBy} onValueChange={setSortBy}>
              <SelectTrigger
                className="w-45 rounded-full"
                aria-label="Sort options"
              >
                <SelectValue placeholder="Sort by" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="name">Name (A-Z)</SelectItem>
                <SelectItem value="price-low">Price (Low to High)</SelectItem>
                <SelectItem value="price-high">Price (High to Low)</SelectItem>
                <SelectItem value="rating">Rating</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>

        {/* Book Grid */}
        {isLoading ? (
          <div
            className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 mb-12"
            role="status"
            aria-label="Loading books"
          >
            {Array.from({ length: 8 }).map((_: any, i: number) => (
              <BookCardSkeleton key={i} />
            ))}
          </div>
        ) : books.length === 0 ? (
          <div
            className="flex flex-col items-center justify-center py-24 text-center"
            role="region"
            aria-labelledby="no-results"
          >
            <h2
              id="no-results"
              className="text-3xl font-serif font-medium mb-4"
            >
              No Books Found
            </h2>
            <p className="text-muted-foreground mb-8 max-w-md">
              We couldn't find any books matching your filters. Try adjusting
              your selection or browse all our titles.
            </p>
            <Button
              variant="outline"
              onClick={clearAllFilters}
              className="rounded-full bg-transparent"
            >
              Clear Filters
            </Button>
          </div>
        ) : (
          <>
            <div
              className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 mb-12"
              role="list"
            >
              {books.map((book: any) => (
                <article
                  key={book.id}
                  className="border-none shadow-none group cursor-pointer bg-transparent"
                  role="listitem"
                  itemScope
                  itemType="https://schema.org/Book"
                >
                  <Link href={`/shop/${book.id}`}>
                    <Card className="border-none shadow-none group cursor-pointer bg-transparent">
                      <CardContent className="p-0">
                        <div className="aspect-3/4 overflow-hidden rounded-lg mb-4 bg-secondary relative">
                          {book.priceSale && (
                            <div className="absolute top-2 left-2 z-10 bg-destructive text-destructive-foreground text-[10px] font-bold px-2 py-1 rounded-full uppercase tracking-wider">
                              Sale
                            </div>
                          )}
                          <img
                            src={book.imageUrl || "/placeholder.svg"}
                            alt={book.name}
                            className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
                          />
                        </div>
                        <div className="space-y-1">
                          <div className="flex items-center justify-between">
                            <p className="text-[10px] uppercase tracking-widest text-muted-foreground font-bold">
                              {book.category?.name}
                            </p>
                            <div className="flex items-center text-xs gap-0.5">
                              <Star className="size-3 fill-primary text-primary" />
                              <span>
                                {book.averageRating?.toFixed(1) ?? "0.0"}
                              </span>
                            </div>
                          </div>
                          <h3 className="font-serif font-medium text-lg leading-snug group-hover:underline">
                            {book.name}
                          </h3>
                          <p className="text-sm text-muted-foreground">
                            {book.authors?.map((a: any) => a.name).join(", ")}
                          </p>
                          <div className="flex items-center gap-2 pt-1">
                            {book.priceSale ? (
                              <>
                                <span className="font-bold text-primary">
                                  ${book.priceSale.toFixed(2)}
                                </span>
                                <span className="text-sm text-muted-foreground line-through decoration-muted-foreground/50">
                                  ${book.price.toFixed(2)}
                                </span>
                              </>
                            ) : (
                              <span className="font-bold">
                                ${book.price.toFixed(2)}
                              </span>
                            )}
                          </div>
                        </div>
                      </CardContent>
                    </Card>
                  </Link>
                </article>
              ))}
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <nav
                className="flex justify-center items-center gap-2"
                role="navigation"
                aria-label="Pagination"
              >
                <Button
                  variant="outline"
                  size="sm"
                  disabled={currentPage === 1}
                  onClick={() => setCurrentPage(currentPage - 1)}
                  className="rounded-full"
                  aria-label="Previous page"
                >
                  Previous
                </Button>

                <div className="flex gap-2">
                  {getPageNumbers().map((page, idx) =>
                    typeof page === "number" ? (
                      <Link
                        key={idx}
                        href={createPageLink(page)}
                        onClick={(e) => {
                          e.preventDefault();
                          setCurrentPage(page);
                        }}
                      >
                        <Button
                          variant={currentPage === page ? "default" : "outline"}
                          size="sm"
                          className="rounded-full min-w-10"
                          aria-label={`Page ${page}`}
                          aria-current={
                            currentPage === page ? "page" : undefined
                          }
                        >
                          {page}
                        </Button>
                      </Link>
                    ) : (
                      <span
                        key={idx}
                        className="px-3 py-2 text-sm text-muted-foreground"
                      >
                        {page}
                      </span>
                    ),
                  )}
                </div>

                <Button
                  variant="outline"
                  size="sm"
                  disabled={currentPage === totalPages}
                  onClick={() => setCurrentPage(currentPage + 1)}
                  className="rounded-full"
                  aria-label="Next page"
                >
                  Next
                </Button>
              </nav>
            )}
          </>
        )}
      </section>
    </div>
  );
}

export default function ShopPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [currentPage, setCurrentPage] = useState(1);
  const [priceRange, setPriceRange] = useState([0, 100]);
  const [debouncedPriceRange] = useDebounceValue(priceRange, 300);
  const [selectedAuthors, setSelectedAuthors] = useState<string[]>([]);
  const [selectedPublishers, setSelectedPublishers] = useState<string[]>([]);
  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
  const [searchQuery, setSearchQuery] = useState<string>("");
  const [sortBy, setSortBy] = useState("name");
  const [isFilterOpen, setIsFilterOpen] = useState(false);

  // Initialize filters from URL params on mount
  useEffect(() => {
    const categoryParam = searchParams.get("category");
    const publisherParam = searchParams.get("publisher");
    const authorParam = searchParams.get("author");
    const searchParam = searchParams.get("search");
    const pageParam = searchParams.get("page");

    if (categoryParam) {
      setSelectedCategories([categoryParam]);
    } else {
      setSelectedCategories([]);
    }
    if (publisherParam) {
      setSelectedPublishers([publisherParam]);
    } else {
      setSelectedPublishers([]);
    }
    if (authorParam) {
      setSelectedAuthors([authorParam]);
    } else {
      setSelectedAuthors([]);
    }
    if (searchParam) {
      setSearchQuery(searchParam);
    } else {
      setSearchQuery("");
    }
    if (pageParam) {
      setCurrentPage(parseInt(pageParam, 10));
    } else {
      setCurrentPage(1);
    }
  }, [searchParams]);

  // Convert sortBy to API parameters
  const getSortParams = () => {
    switch (sortBy) {
      case "price-low":
        return { orderBy: "price", isDescending: false };
      case "price-high":
        return { orderBy: "price", isDescending: true };
      case "rating":
        return { orderBy: "averageRating", isDescending: true };
      case "name":
      default:
        return { orderBy: "name", isDescending: false };
    }
  };

  const sortParams = getSortParams();

  // API hooks with windowed pagination
  const { data: booksData, isLoading: isLoadingBooks } = useBooks({
    pageIndex: currentPage,
    pageSize: ITEMS_PER_PAGE,
    categoryId: selectedCategories[0],
    publisherId: selectedPublishers[0],
    authorId: selectedAuthors[0],
    search: searchQuery || undefined,
    minPrice: priceRange[0],
    maxPrice: priceRange[1],
    orderBy: sortParams.orderBy,
    isDescending: sortParams.isDescending,
  });
  const { data: categories } = useCategories();
  const { data: publishers } = usePublishers();
  const { data: authors } = useAuthors();

  const isLoading = isLoadingBooks;
  const [expandedSections, setExpandedSections] = useState({
    categories: false,
    publishers: false,
    authors: false,
  });
  const [filterSearches, setFilterSearches] = useState({
    categories: "",
    publishers: "",
    authors: "",
  });
  const [searchOpen, setSearchOpen] = useState({
    categories: false,
    publishers: false,
    authors: false,
  });

  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="grow container mx-auto px-4 py-8">
        <ShopContent
          booksData={booksData}
          categories={categories}
          publishers={publishers}
          authors={authors}
          searchQuery={searchQuery}
          priceRange={priceRange}
          setPriceRange={setPriceRange}
          selectedAuthors={selectedAuthors}
          setSelectedAuthors={setSelectedAuthors}
          selectedPublishers={selectedPublishers}
          setSelectedPublishers={setSelectedPublishers}
          selectedCategories={selectedCategories}
          setSelectedCategories={setSelectedCategories}
          sortBy={sortBy}
          setSortBy={setSortBy}
          isLoading={isLoading}
          isFilterOpen={isFilterOpen}
          setIsFilterOpen={setIsFilterOpen}
          expandedSections={expandedSections}
          setExpandedSections={setExpandedSections}
          filterSearches={filterSearches}
          setFilterSearches={setFilterSearches}
          searchOpen={searchOpen}
          setSearchOpen={setSearchOpen}
          currentPage={currentPage}
          setCurrentPage={setCurrentPage}
        />
      </main>
      <Footer />
    </div>
  );
}
