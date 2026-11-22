import { Skeleton } from "@workspace/ui/components/skeleton";

export function BookCardSkeleton() {
  return (
    <output className="space-y-4" aria-label="Loading book card">
      <Skeleton className="aspect-3/4 w-full rounded-lg" />
      <Skeleton className="h-3 w-16 rounded-full" />
      <Skeleton className="h-6 w-full rounded-md" />
      <Skeleton className="h-4 w-24 rounded-md" />
      <Skeleton className="h-5 w-20 rounded-md" />
      <span className="sr-only">Loading book information...</span>
    </output>
  );
}

export function CategoryCardSkeleton() {
  return (
    <output
      className="bg-background border-border/40 flex items-center justify-between border p-8 md:p-12"
      aria-label="Loading category"
    >
      <div className="flex flex-1 flex-col gap-4">
        <Skeleton className="h-4 w-16" />
        <Skeleton className="h-10 w-48" />
      </div>
      <Skeleton className="size-12 rounded-full" />
      <span className="sr-only">Loading category information...</span>
    </output>
  );
}

export function PublisherCardSkeleton() {
  return (
    <output
      className="bg-secondary/10 border-border/40 relative flex aspect-square flex-col justify-between overflow-hidden rounded-lg border p-8"
      aria-label="Loading publisher"
    >
      <Skeleton className="mb-4 h-7 w-28 rounded-full" />
      <div className="space-y-3">
        <Skeleton className="h-8 w-full rounded-md" />
        <Skeleton className="h-8 w-3/4 rounded-md" />
        <Skeleton className="mt-4 h-5 w-20 rounded-md" />
      </div>
      <span className="sr-only">Loading publisher information...</span>
    </output>
  );
}

export function ReviewsLoadingSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-10 w-1/3 rounded-lg" />
      <Skeleton className="h-6 w-1/2 rounded-lg" />
      <div className="space-y-8">
        {Array.from({ length: 3 }, (_, idx) => idx).map((i) => (
          <div key={i} className="space-y-4">
            <div className="flex gap-3">
              <Skeleton className="size-10 shrink-0 rounded-full" />
              <div className="flex-1 space-y-2">
                <Skeleton className="h-4 w-1/4 rounded" />
                <div className="flex gap-1">
                  {Array.from({ length: 5 }, (_, idx) => idx).map((j) => (
                    <Skeleton key={j} className="size-3 rounded" />
                  ))}
                </div>
              </div>
            </div>
            <Skeleton className="h-16 rounded" />
          </div>
        ))}
      </div>
    </div>
  );
}

export function ProductLoadingSkeleton() {
  return (
    <div className="space-y-8">
      <Skeleton className="h-6 w-32 rounded" />
      <div className="grid grid-cols-1 gap-12 md:grid-cols-2">
        <Skeleton className="aspect-3/4 rounded-2xl" />
        <div className="space-y-6">
          <Skeleton className="h-8 w-3/4 rounded" />
          <Skeleton className="h-6 w-1/2 rounded" />
          <Skeleton className="h-24 rounded" />
        </div>
      </div>
    </div>
  );
}
