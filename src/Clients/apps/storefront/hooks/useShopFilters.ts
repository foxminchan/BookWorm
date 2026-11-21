"use client";

import { useEffect, useState } from "react";

import { useSearchParams } from "next/navigation";

export function useShopFilters() {
  const searchParams = useSearchParams();
  const [currentPage, setCurrentPage] = useState(() => {
    const pageParam = searchParams.get("page");
    return pageParam ? Number.parseInt(pageParam, 10) : 1;
  });
  const [priceRange, setPriceRange] = useState([0, 100]);
  const [selectedAuthors, setSelectedAuthors] = useState<string[]>(() => {
    const authorParam = searchParams.get("author");
    return authorParam ? [authorParam] : [];
  });
  const [selectedPublishers, setSelectedPublishers] = useState<string[]>(() => {
    const publisherParam = searchParams.get("publisher");
    return publisherParam ? [publisherParam] : [];
  });
  const [selectedCategories, setSelectedCategories] = useState<string[]>(() => {
    const categoryParam = searchParams.get("category");
    return categoryParam ? [categoryParam] : [];
  });
  const [searchQuery, setSearchQuery] = useState<string>(
    () => searchParams.get("search") ?? "",
  );
  const [sortBy, setSortBy] = useState("name");
  const [isFilterOpen, setIsFilterOpen] = useState(false);

  const searchParamsKey = searchParams.toString();

  useEffect(() => {
    const categoryParam = searchParams.get("category");
    const publisherParam = searchParams.get("publisher");
    const authorParam = searchParams.get("author");
    const searchParam = searchParams.get("search");
    const pageParam = searchParams.get("page");

    setSelectedCategories(categoryParam ? [categoryParam] : []);
    setSelectedPublishers(publisherParam ? [publisherParam] : []);
    setSelectedAuthors(authorParam ? [authorParam] : []);
    setSearchQuery(searchParam ?? "");
    setCurrentPage(pageParam ? Number.parseInt(pageParam, 10) : 1);
  }, [searchParamsKey, searchParams]);

  return {
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
    setSearchQuery,
    sortBy,
    setSortBy,
    isFilterOpen,
    setIsFilterOpen,
  };
}
