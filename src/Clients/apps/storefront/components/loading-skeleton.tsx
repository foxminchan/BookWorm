import { Skeleton } from "@workspace/ui/components/skeleton";

export function BookCardSkeleton() {
  return (
    <div className="space-y-4">
      <Skeleton className="aspect-3/4 w-full rounded-lg" />
      <Skeleton className="h-3 w-16 rounded-full" />
      <Skeleton className="h-6 w-full rounded-md" />
      <Skeleton className="h-4 w-24 rounded-md" />
      <Skeleton className="h-5 w-20 rounded-md" />
    </div>
  );
}

export function CategoryCardSkeleton() {
  return (
    <div className="flex items-center justify-between p-8 md:p-12 bg-background border border-border/40">
      <div className="flex flex-col gap-4 flex-1">
        <Skeleton className="h-4 w-16" />
        <Skeleton className="h-10 w-48" />
      </div>
      <Skeleton className="size-12 rounded-full" />
    </div>
  );
}

export function PublisherCardSkeleton() {
  return (
    <div className="relative aspect-square rounded-lg overflow-hidden bg-secondary/10 border border-border/40 p-8 flex flex-col justify-between">
      <Skeleton className="h-7 w-28 rounded-full mb-4" />
      <div className="space-y-3">
        <Skeleton className="h-8 w-full rounded-md" />
        <Skeleton className="h-8 w-3/4 rounded-md" />
        <Skeleton className="h-5 w-20 rounded-md mt-4" />
      </div>
    </div>
  );
}

export function BasketItemSkeleton() {
  return (
    <div className="border-none shadow-none bg-white/50 dark:bg-gray-800/50 p-6 rounded-lg">
      <div className="flex gap-6">
        <div className="grow space-y-4">
          <div className="flex justify-between items-start">
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-5 w-5 rounded-full" />
          </div>
          <Skeleton className="h-4 w-20" />
          <div className="flex items-center gap-3 pt-4">
            <Skeleton className="h-10 w-32" />
            <Skeleton className="h-6 w-24 ml-auto" />
          </div>
        </div>
      </div>
    </div>
  );
}
