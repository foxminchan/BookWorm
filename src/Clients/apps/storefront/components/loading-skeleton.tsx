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

export function ConfirmationPageSkeleton() {
  return (
    <div className="max-w-3xl mx-auto">
      <div className="text-center mb-24 space-y-8">
        <div className="flex justify-center">
          <Skeleton className="size-24 rounded-full" />
        </div>
        <Skeleton className="h-16 w-96 mx-auto" />
        <Skeleton className="h-8 w-64 mx-auto" />
        <div className="pt-8">
          <Skeleton className="h-4 w-32 mx-auto mb-2" />
          <Skeleton className="h-10 w-40 mx-auto" />
        </div>
      </div>

      <div className="grid md:grid-cols-3 gap-8 mb-24 py-12 border-y border-border">
        {[1, 2, 3].map((i) => (
          <div key={i} className="space-y-4">
            <Skeleton className="h-6 w-32" />
            <div className="space-y-2">
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-3/4" />
              <Skeleton className="h-4 w-5/6" />
            </div>
          </div>
        ))}
      </div>

      <Skeleton className="h-20 w-full mb-12 rounded-xl" />

      <div className="flex flex-col sm:flex-row items-center justify-center gap-6">
        <Skeleton className="h-14 w-64 rounded-full" />
        <Skeleton className="h-14 w-48 rounded-full" />
      </div>
    </div>
  );
}
