"use client";

import { X } from "lucide-react";

import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";
import { Badge } from "@workspace/ui/components/badge";
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
      <div className="flex flex-wrap gap-2">
        {authors?.map((author) => (
          <Badge
            key={author.id}
            variant={
              selectedAuthors.includes(author.id) ? "default" : "outline"
            }
            className="cursor-pointer"
            onClick={() => onToggle(author.id)}
          >
            {author.name}
          </Badge>
        ))}
      </div>
      {selectedAuthors.length > 0 && (
        <Button
          variant="link"
          size="sm"
          onClick={onClear}
          className="text-muted-foreground hover:text-foreground mt-2 flex items-center gap-1 text-xs"
        >
          <X className="h-3 w-3" />
          Clear authors
        </Button>
      )}
    </div>
  );
}
