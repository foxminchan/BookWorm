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

export function OrdersSkeleton() {
  return (
    <div className="space-y-px border border-border/40 bg-border/40">
      {Array.from({ length: 5 }).map((_, i) => (
        <div
          key={i}
          className="bg-background p-8 border-b border-border/40 last:border-b-0"
        >
          <div className="space-y-4">
            <div className="flex items-center gap-4">
              <Skeleton className="h-8 w-32" />
              <Skeleton className="h-6 w-20" />
            </div>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-6 pt-6 border-t border-border">
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

export function OrderDetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="space-y-4">
        <Skeleton className="h-10 w-32" />
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-48" />
      </div>

      {/* Order Items Skeleton */}
      <div className="border border-border/40 rounded-lg overflow-hidden">
        <div className="border-b border-border/40 px-6 py-4">
          <Skeleton className="h-6 w-32" />
        </div>
        <div className="divide-y divide-border/40">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="px-6 py-4 space-y-3">
              <Skeleton className="h-5 w-48" />
              <Skeleton className="h-4 w-32" />
            </div>
          ))}
        </div>
      </div>

      {/* Order Summary Skeleton */}
      <div className="border border-border/40 rounded-lg p-6">
        <Skeleton className="h-6 w-32 mb-6" />
        <div className="space-y-3">
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-6 w-full" />
        </div>
      </div>
    </div>
  );
}

export function AccountPageSkeleton() {
  return (
    <div>
      <div className="mb-12 md:mb-16">
        <Skeleton className="h-16 w-64 mb-3" />
        <Skeleton className="h-6 w-96" />
      </div>

      <div className="grid gap-8 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-8">
          {/* Profile Section Skeleton */}
          <div className="border border-border/40 rounded-lg p-6">
            <div className="flex items-center gap-4">
              <Skeleton className="size-16 rounded-full" />
              <div className="space-y-2 flex-1">
                <Skeleton className="h-6 w-32" />
                <Skeleton className="h-4 w-48" />
              </div>
            </div>
          </div>

          {/* Delivery Address Section Skeleton */}
          <div className="border border-border/40 rounded-lg p-8">
            <div className="flex items-center justify-between mb-6">
              <div className="flex items-center gap-3">
                <Skeleton className="size-10 rounded-lg" />
                <Skeleton className="h-6 w-40" />
              </div>
              <Skeleton className="h-9 w-20" />
            </div>
            <Skeleton className="h-4 w-full" />
          </div>
        </div>

        <div className="lg:col-span-1">
          <div className="sticky top-24 space-y-6">
            {/* Navigation Card Skeleton */}
            <div className="border border-border/40 rounded-lg overflow-hidden">
              <div className="p-4">
                <Skeleton className="h-6 w-32" />
              </div>
            </div>

            {/* Logout Button Skeleton */}
            <Skeleton className="h-12 w-full rounded-lg" />
          </div>
        </div>
      </div>
    </div>
  );
}

export function ReviewsLoadingSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-10 w-1/3 rounded-lg" />
      <Skeleton className="h-6 w-1/2 rounded-lg" />
      <div className="space-y-8">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="space-y-4">
            <div className="flex gap-3">
              <Skeleton className="size-10 rounded-full shrink-0" />
              <div className="flex-1 space-y-2">
                <Skeleton className="h-4 w-1/4 rounded" />
                <div className="flex gap-1">
                  {Array.from({ length: 5 }).map((_, j) => (
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
      <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
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
