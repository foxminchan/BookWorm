"use client";

import { X } from "lucide-react";

import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";
import { Button } from "@workspace/ui/components/button";
import { Label } from "@workspace/ui/components/label";
import { Skeleton } from "@workspace/ui/components/skeleton";

type AuthorsFilterProps = Readonly<{
  selectedAuthors: string[];
  onToggle: (authorId: string) => void;
  onClear: () => void;
}>;

const SKELETON_KEYS = Array.from(
  { length: 6 },
  (_, i) => `author-skeleton-${i}`,
);

export function AuthorsFilter({
  selectedAuthors,
  onToggle,
  onClear,
}: AuthorsFilterProps) {
  const { data: authors, isLoading } = useAuthors();

  if (isLoading) {
    return (
      <div className="space-y-2">
        <Skeleton className="h-4 w-16" />
        <div className="flex flex-wrap gap-2">
          {SKELETON_KEYS.map((key) => (
            <Skeleton key={key} className="h-6 w-20" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <Label className="text-xs">Authors</Label>
      <fieldset className="flex flex-wrap gap-2" aria-label="Filter by authors">
        <legend className="sr-only">Filter by authors</legend>
        {authors?.map((author) => {
          const isSelected = selectedAuthors.includes(author.id);
          return (
            <Button
              key={author.id}
              variant={isSelected ? "default" : "outline"}
              size="sm"
              className="h-6 rounded-full px-3 text-xs"
              onClick={() => onToggle(author.id)}
              aria-pressed={isSelected}
              aria-label={`${isSelected ? "Remove" : "Add"} ${author.name} filter`}
            >
              {author.name}
            </Button>
          );
        })}
      </fieldset>
      {selectedAuthors.length > 0 && (
        <Button
          variant="link"
          size="sm"
          onClick={onClear}
          className="text-muted-foreground hover:text-foreground mt-2 flex items-center gap-1 text-xs"
          aria-label="Clear all author filters"
        >
          <X className="h-3 w-3" aria-hidden="true" />
          Clear authors
        </Button>
      )}
    </div>
  );
}
