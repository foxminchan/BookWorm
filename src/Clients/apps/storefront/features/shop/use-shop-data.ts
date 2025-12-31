"use client";

import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";
import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";
import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";

const ITEMS_PER_PAGE = 8;

type UseShopDataParams = {
  currentPage: number;
  priceRange: number[];
  selectedCategories: string[];
  selectedPublishers: string[];
  selectedAuthors: string[];
  searchQuery: string;
  sortBy: string;
};

// Convert sortBy to API parameters
function getSortParams(sortBy: string) {
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
}

export function useShopData({
  currentPage,
  priceRange,
  selectedCategories,
  selectedPublishers,
  selectedAuthors,
  searchQuery,
  sortBy,
}: UseShopDataParams) {
  const sortParams = getSortParams(sortBy);

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

  return {
    booksData,
    categories,
    publishers,
    authors,
    isLoading: isLoadingBooks,
  };
}
