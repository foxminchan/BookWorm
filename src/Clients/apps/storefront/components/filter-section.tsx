"use client";

import { useState } from "react";

import { Search } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import { FieldLegend, FieldSet } from "@workspace/ui/components/field";
import { Input } from "@workspace/ui/components/input";
import { Label } from "@workspace/ui/components/label";

import { FilterCheckbox } from "@/components/filter-checkbox";

type FilterItem = {
  id: string;
  name: string;
};

type FilterSectionProps = {
  title: string;
  items: FilterItem[];
  selectedItems: string[];
  onToggle: (id: string) => void;
  searchable?: boolean;
  collapsible?: boolean;
  maxVisibleItems?: number;
};

export function FilterSection({
  title,
  items,
  selectedItems,
  onToggle,
  searchable = true,
  collapsible = true,
  maxVisibleItems = 5,
}: Readonly<FilterSectionProps>) {
  const [isExpanded, setIsExpanded] = useState(false);
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const sectionId = title.toLowerCase().replaceAll(/\s+/g, "-");

  const filteredItems = items.filter((item) =>
    item.name.toLowerCase().includes(searchTerm.toLowerCase()),
  );

  const displayedItems =
    collapsible && !isExpanded
      ? filteredItems.slice(0, maxVisibleItems)
      : filteredItems;

  const showToggle = collapsible && filteredItems.length > maxVisibleItems;

  return (
    <div>
      <div className="mb-2 flex items-center justify-between gap-2">
        <h3 className="font-serif font-semibold">{title}</h3>
        {searchable && (
          <Button
            type="button"
            variant="ghost"
            size="icon"
            onClick={() => setIsSearchOpen(!isSearchOpen)}
            className="text-muted-foreground hover:text-foreground h-8 w-8"
            aria-label={`Search ${title.toLowerCase()}`}
            aria-expanded={isSearchOpen}
          >
            <Search className="size-4" aria-hidden="true" />
          </Button>
        )}
      </div>

      {isSearchOpen && (
        <div className="relative mb-2">
          <Label htmlFor={`filter-search-${sectionId}`} className="sr-only">
            Search {title.toLowerCase()}
          </Label>
          <Search
            className="text-muted-foreground absolute top-1/2 left-2 size-4 -translate-y-1/2"
            aria-hidden="true"
          />
          <Input
            id={`filter-search-${sectionId}`}
            placeholder={`Search ${title.toLowerCase()}...`}
            className="h-8 pl-8 text-sm"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            autoFocus
            aria-describedby={`${sectionId}-filter-description`}
          />
          <span id={`${sectionId}-filter-description`} className="sr-only">
            Filter {title.toLowerCase()} list
          </span>
        </div>
      )}

      <FieldSet
        id={`${sectionId}-filter-list`}
        className={`gap-1 overflow-hidden border-none p-0 transition-all duration-300 ease-in-out ${
          isExpanded ? "max-h-96" : "max-h-56"
        }`}
      >
        <FieldLegend className="sr-only">{title} filters</FieldLegend>
        {displayedItems.map((item) => (
          <FilterCheckbox
            key={item.id}
            label={item.name}
            checked={selectedItems.includes(item.id)}
            onChange={() => onToggle(item.id)}
          />
        ))}
      </FieldSet>

      {showToggle && (
        <>
          <Button
            variant="link"
            onClick={() => setIsExpanded(!isExpanded)}
            className="text-primary mt-2 h-auto p-0 text-sm hover:underline"
            aria-expanded={isExpanded}
            aria-controls={`${sectionId}-filter-list`}
          >
            {isExpanded ? "Show Less" : "Show More"}
          </Button>
          <span className="sr-only" aria-live="polite" aria-atomic="true">
            {isExpanded ? "Showing all filters" : "Showing limited filters"}
          </span>
        </>
      )}
    </div>
  );
}
