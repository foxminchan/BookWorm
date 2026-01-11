"use client";

import { X } from "lucide-react";

import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";
import { Button } from "@workspace/ui/components/button";
import { Label } from "@workspace/ui/components/label";
import { Skeleton } from "@workspace/ui/components/skeleton";

type AuthorsFilterProps = {
  selectedAuthors: string[];
  onToggle: (authorId: string) => void;
  onClear: () => void;
};

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
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-6 w-20" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <Label className="text-xs">Authors</Label>
      <div
        className="flex flex-wrap gap-2"
        role="group"
        aria-label="Filter by authors"
      >
        {authors?.map((author) => (
          <Button
            key={author.id}
            variant={
              selectedAuthors.includes(author.id) ? "default" : "outline"
            }
            size="sm"
            className="h-6 rounded-full px-3 text-xs"
            onClick={() => onToggle(author.id)}
            aria-pressed={selectedAuthors.includes(author.id)}
            aria-label={`${selectedAuthors.includes(author.id) ? "Remove" : "Add"} ${author.name} filter`}
          >
            {author.name}
          </Button>
        ))}
      </div>
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
