"use client";

import { useState } from "react";
import { Search } from "lucide-react";
import { Input } from "@workspace/ui/components/input";
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
}: FilterSectionProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");

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
      <div className="flex items-center justify-between gap-2 mb-4">
        <h3 className="font-serif font-medium">{title}</h3>
        {searchable && (
          <button
            onClick={() => setIsSearchOpen(!isSearchOpen)}
            className="text-muted-foreground hover:text-foreground transition"
            aria-label={`Search ${title.toLowerCase()}`}
          >
            <Search className="size-4" />
          </button>
        )}
      </div>

      {isSearchOpen && (
        <div className="relative mb-4">
          <Search className="absolute left-2 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input
            placeholder={`Search ${title.toLowerCase()}...`}
            className="pl-8 h-8 text-sm"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            autoFocus
          />
        </div>
      )}

      <div
        className={`space-y-2 overflow-hidden transition-all duration-300 ease-in-out ${
          isExpanded ? "max-h-96" : "max-h-56"
        }`}
      >
        {displayedItems.map((item) => (
          <FilterCheckbox
            key={item.id}
            label={item.name}
            checked={selectedItems.includes(item.id)}
            onChange={() => onToggle(item.id)}
          />
        ))}
      </div>

      {showToggle && (
        <button
          onClick={() => setIsExpanded(!isExpanded)}
          className="text-sm text-primary hover:underline mt-2 transition-colors"
        >
          {isExpanded ? "Show Less" : "Show More"}
        </button>
      )}
    </div>
  );
}
