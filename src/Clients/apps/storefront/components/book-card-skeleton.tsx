import { Skeleton } from "@workspace/ui/components/skeleton";

export function BookCardSkeleton() {
  return (
    <div className="space-y-4">
      {/* Image skeleton */}
      <Skeleton className="aspect-3/4 w-full rounded-lg" />
      {/* Category skeleton */}
      <Skeleton className="h-3 w-16 rounded-full" />
      {/* Title skeleton */}
      <Skeleton className="h-6 w-full rounded-md" />
      {/* Author skeleton */}
      <Skeleton className="h-4 w-24 rounded-md" />
      {/* Price skeleton */}
      <Skeleton className="h-5 w-20 rounded-md" />
    </div>
  );
}
