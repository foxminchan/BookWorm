"use client";

import { useState, useMemo } from "react";
import { useDebounceValue } from "./use-debounce-value";
import { DEBOUNCE_DELAY } from "@/lib/constants";

interface UseFiltersOptions<T> {
  items: T[];
  initialPriceRange?: [number, number];
  filterFn: (
    item: T,
    filters: {
      categories: string[];
      publishers: string[];
      authors: string[];
      priceRange: [number, number];
    },
  ) => boolean;
}

export function useFilters<T>({
  items,
  initialPriceRange = [0, 100],
  filterFn,
}: UseFiltersOptions<T>) {
  const [priceRange, setPriceRange] =
    useState<[number, number]>(initialPriceRange);
  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
  const [selectedPublishers, setSelectedPublishers] = useState<string[]>([]);
  const [selectedAuthors, setSelectedAuthors] = useState<string[]>([]);

  const debouncedPriceRange = useDebounceValue(
    priceRange,
    DEBOUNCE_DELAY.priceRange,
  );

  const filteredItems = useMemo(() => {
    return items.filter((item) =>
      filterFn(item, {
        categories: selectedCategories,
        publishers: selectedPublishers,
        authors: selectedAuthors,
        priceRange: debouncedPriceRange,
      }),
    );
  }, [
    items,
    selectedCategories,
    selectedPublishers,
    selectedAuthors,
    debouncedPriceRange,
    filterFn,
  ]);

  const toggleCategory = (id: string) => {
    setSelectedCategories((prev) =>
      prev.includes(id) ? prev.filter((c) => c !== id) : [...prev, id],
    );
  };

  const togglePublisher = (id: string) => {
    setSelectedPublishers((prev) =>
      prev.includes(id) ? prev.filter((p) => p !== id) : [...prev, id],
    );
  };

  const toggleAuthor = (name: string) => {
    setSelectedAuthors((prev) =>
      prev.includes(name) ? prev.filter((a) => a !== name) : [...prev, name],
    );
  };

  const clearFilters = () => {
    setSelectedCategories([]);
    setSelectedPublishers([]);
    setSelectedAuthors([]);
    setPriceRange(initialPriceRange);
  };

  const hasActiveFilters =
    selectedCategories.length > 0 ||
    selectedPublishers.length > 0 ||
    selectedAuthors.length > 0 ||
    priceRange[0] !== initialPriceRange[0] ||
    priceRange[1] !== initialPriceRange[1];

  return {
    // State
    priceRange,
    selectedCategories,
    selectedPublishers,
    selectedAuthors,
    // Derived
    filteredItems,
    hasActiveFilters,
    // Actions
    setPriceRange,
    toggleCategory,
    togglePublisher,
    toggleAuthor,
    clearFilters,
  };
}
