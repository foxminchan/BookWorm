import { Skeleton } from "@workspace/ui/components/skeleton";

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
