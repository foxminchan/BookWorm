"use client";

import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";
import { Label } from "@workspace/ui/components/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";
import { Skeleton } from "@workspace/ui/components/skeleton";

type PublisherSelectProps = {
  value: string | undefined;
  onChange: (value: string | undefined) => void;
};

export function PublisherSelect({ value, onChange }: PublisherSelectProps) {
  const { data: publishers, isLoading } = usePublishers();

  if (isLoading) {
    return (
      <div className="space-y-2">
        <Skeleton className="h-4 w-20" />
        <Skeleton className="h-10 w-full" />
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <Label htmlFor="publisher-select" className="text-xs">
        Publisher
      </Label>
      <Select
        value={value ?? ""}
        onValueChange={(val) => onChange(val || undefined)}
      >
        <SelectTrigger id="publisher-select" className="w-full">
          <SelectValue placeholder="All Publishers" />
        </SelectTrigger>
        <SelectContent>
          {publishers?.map((pub) => (
            <SelectItem key={pub.id} value={pub.id}>
              {pub.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
