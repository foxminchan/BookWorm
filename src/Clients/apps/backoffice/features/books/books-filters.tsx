"use client";

import { useCallback, useEffect, useReducer, useState } from "react";

import { ChevronDown, ChevronUp } from "lucide-react";
import { useDebounceCallback } from "usehooks-ts";

import type { ListBooksQuery } from "@workspace/types/catalog/books";
import { Button } from "@workspace/ui/components/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@workspace/ui/components/collapsible";
import { PRICE_RANGE } from "@workspace/utils/constants";

import { AuthorsFilter } from "./filters/authors-filter";
import { CategorySelect } from "./filters/category-select";
import { PriceRangeFilter } from "./filters/price-range-filter";
import { PublisherSelect } from "./filters/publisher-select";
import { SearchInput } from "./filters/search-input";

type BooksFiltersProps = Readonly<{
  onFiltersChange: (
    query: Omit<ListBooksQuery, "pageIndex" | "pageSize">,
  ) => void;
}>;

type FilterState = {
  searchTerm: string;
  debouncedSearch: string;
  priceRange: [number, number];
  debouncedPriceRange: [number, number];
  orderBy: string;
  isDescending: boolean;
  selectedCategory: string | undefined;
  selectedAuthors: string[];
  debouncedAuthors: string[];
  selectedPublisher: string | undefined;
};

type FilterAction =
  | { type: "SET_SEARCH_TERM"; payload: string }
  | { type: "SET_DEBOUNCED_SEARCH"; payload: string }
  | { type: "SET_PRICE_RANGE"; payload: [number, number] }
  | { type: "SET_DEBOUNCED_PRICE_RANGE"; payload: [number, number] }
  | { type: "SET_CATEGORY"; payload: string | undefined }
  | { type: "SET_AUTHORS"; payload: string[] }
  | { type: "SET_DEBOUNCED_AUTHORS"; payload: string[] }
  | { type: "SET_PUBLISHER"; payload: string | undefined }
  | { type: "CLEAR_AUTHORS" }
  | { type: "RESET" };

const initialFilterState: FilterState = {
  searchTerm: "",
  debouncedSearch: "",
  priceRange: [PRICE_RANGE.min, PRICE_RANGE.max],
  debouncedPriceRange: [PRICE_RANGE.min, PRICE_RANGE.max],
  orderBy: "name",
  isDescending: false,
  selectedCategory: undefined,
  selectedAuthors: [],
  debouncedAuthors: [],
  selectedPublisher: undefined,
};

function filterReducer(state: FilterState, action: FilterAction): FilterState {
  switch (action.type) {
    case "SET_SEARCH_TERM":
      return { ...state, searchTerm: action.payload };
    case "SET_DEBOUNCED_SEARCH":
      return { ...state, debouncedSearch: action.payload };
    case "SET_PRICE_RANGE":
      return { ...state, priceRange: action.payload };
    case "SET_DEBOUNCED_PRICE_RANGE":
      return { ...state, debouncedPriceRange: action.payload };
    case "SET_CATEGORY":
      return { ...state, selectedCategory: action.payload };
    case "SET_AUTHORS":
      return { ...state, selectedAuthors: action.payload };
    case "SET_DEBOUNCED_AUTHORS":
      return { ...state, debouncedAuthors: action.payload };
    case "SET_PUBLISHER":
      return { ...state, selectedPublisher: action.payload };
    case "CLEAR_AUTHORS":
      return { ...state, selectedAuthors: [], debouncedAuthors: [] };
    case "RESET":
      return initialFilterState;
    default:
      return state;
  }
}

export function BooksFilters({ onFiltersChange }: BooksFiltersProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const [state, dispatch] = useReducer(filterReducer, initialFilterState);

  // Sync query whenever any debounced/immediate filter value changes
  useEffect(() => {
    const query: Omit<ListBooksQuery, "pageIndex" | "pageSize"> = {
      search: state.debouncedSearch || undefined,
      minPrice:
        state.debouncedPriceRange[0] === PRICE_RANGE.min
          ? undefined
          : state.debouncedPriceRange[0],
      maxPrice:
        state.debouncedPriceRange[1] === PRICE_RANGE.max
          ? undefined
          : state.debouncedPriceRange[1],
      orderBy: state.orderBy,
      isDescending: state.isDescending,
      categoryId: state.selectedCategory,
      authorId:
        state.debouncedAuthors.length > 0
          ? state.debouncedAuthors[0]
          : undefined,
      publisherId: state.selectedPublisher ?? undefined,
    };
    onFiltersChange(query);
  }, [
    state.debouncedSearch,
    state.debouncedPriceRange,
    state.orderBy,
    state.isDescending,
    state.selectedCategory,
    state.debouncedAuthors,
    state.selectedPublisher,
    onFiltersChange,
  ]);

  const handleSearchChange = useDebounceCallback(
    (term: string) => {
      dispatch({ type: "SET_DEBOUNCED_SEARCH", payload: term });
    },
    500,
    { leading: false, trailing: true },
  );

  const handleDebouncedPriceChange = useDebounceCallback(
    (min: number, max: number) => {
      dispatch({ type: "SET_DEBOUNCED_PRICE_RANGE", payload: [min, max] });
    },
    500,
    { leading: false, trailing: true },
  );

  const handleDebouncedAuthorChange = useDebounceCallback(
    (authors: string[]) => {
      dispatch({ type: "SET_DEBOUNCED_AUTHORS", payload: authors });
    },
    500,
    { leading: false, trailing: true },
  );

  const handleAuthorToggle = useCallback(
    (authorId: string) => {
      const newAuthors = state.selectedAuthors.includes(authorId)
        ? state.selectedAuthors.filter((id) => id !== authorId)
        : [...state.selectedAuthors, authorId];
      dispatch({ type: "SET_AUTHORS", payload: newAuthors });
      handleDebouncedAuthorChange(newAuthors);
    },
    [state.selectedAuthors, handleDebouncedAuthorChange],
  );

  const handlePriceRangeChange = useCallback(
    (newMin: number, newMax: number) => {
      dispatch({ type: "SET_PRICE_RANGE", payload: [newMin, newMax] });
      handleDebouncedPriceChange(newMin, newMax);
    },
    [handleDebouncedPriceChange],
  );

  const handleResetFilters = useCallback(() => {
    dispatch({ type: "RESET" });
  }, []);

  const handleAuthorClear = useCallback(() => {
    dispatch({ type: "CLEAR_AUTHORS" });
  }, []);

  return (
    <Collapsible open={isExpanded} onOpenChange={setIsExpanded} asChild>
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Filter &amp; Search</CardTitle>
            <CollapsibleTrigger asChild>
              <Button variant="ghost" size="sm" className="h-8 w-8 p-0">
                {isExpanded ? (
                  <ChevronUp className="h-4 w-4" />
                ) : (
                  <ChevronDown className="h-4 w-4" />
                )}
                <span className="sr-only">
                  {isExpanded ? "Collapse" : "Expand"} filters
                </span>
              </Button>
            </CollapsibleTrigger>
          </div>
        </CardHeader>
        <CollapsibleContent>
          <CardContent>
            <div className="space-y-4">
              <SearchInput
                value={state.searchTerm}
                onChange={(value) => {
                  dispatch({ type: "SET_SEARCH_TERM", payload: value });
                  handleSearchChange(value);
                }}
              />

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <CategorySelect
                  value={state.selectedCategory}
                  onChange={(value) =>
                    dispatch({ type: "SET_CATEGORY", payload: value })
                  }
                />
                <PublisherSelect
                  value={state.selectedPublisher}
                  onChange={(value) =>
                    dispatch({ type: "SET_PUBLISHER", payload: value })
                  }
                />
              </div>

              <AuthorsFilter
                selectedAuthors={state.selectedAuthors}
                onToggle={handleAuthorToggle}
                onClear={handleAuthorClear}
              />

              <PriceRangeFilter
                min={PRICE_RANGE.min}
                max={PRICE_RANGE.max}
                minPrice={state.priceRange[0]}
                maxPrice={state.priceRange[1]}
                onChange={handlePriceRangeChange}
              />

              <Button
                variant="outline"
                size="sm"
                onClick={handleResetFilters}
                className="w-full bg-transparent"
              >
                Reset Filters
              </Button>
            </div>
          </CardContent>
        </CollapsibleContent>
      </Card>
    </Collapsible>
  );
}
