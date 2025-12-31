import { Skeleton } from "@workspace/ui/components/skeleton";

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
