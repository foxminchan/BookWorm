"use client";

import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";
import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";
import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";

import { getShopSortParams } from "@/lib/pattern";

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

export function useShopData({
  currentPage,
  priceRange,
  selectedCategories,
  selectedPublishers,
  selectedAuthors,
  searchQuery,
  sortBy,
}: UseShopDataParams) {
  const sortParams = getShopSortParams(sortBy);

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
