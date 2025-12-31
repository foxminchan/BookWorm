"use client";

import { useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";

export function useShopFilters() {
  const searchParams = useSearchParams();
  const [currentPage, setCurrentPage] = useState(1);
  const [priceRange, setPriceRange] = useState([0, 100]);
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
