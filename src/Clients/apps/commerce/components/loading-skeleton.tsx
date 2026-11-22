import { Card } from "@workspace/ui/components/card";
import { Separator } from "@workspace/ui/components/separator";
import { Skeleton } from "@workspace/ui/components/skeleton";

export function BasketItemSkeleton() {
  return (
    <output
      className="rounded-lg border-none bg-white/50 p-6 shadow-none dark:bg-gray-800/50"
      aria-label="Loading basket item"
    >
      <div className="flex gap-6">
        <div className="grow space-y-4">
          <div className="flex items-start justify-between">
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-5 w-5 rounded-full" />
          </div>
          <Skeleton className="h-4 w-20" />
          <div className="flex items-center gap-3 pt-4">
            <span className="sr-only">Loading basket item...</span>
            <Skeleton className="h-10 w-32" />
            <Skeleton className="ml-auto h-6 w-24" />
          </div>
        </div>
      </div>
    </output>
  );
}

export default function BasketLoadingSkeleton() {
  return (
    <div className="mx-auto grid max-w-5xl grid-cols-1 gap-12 lg:grid-cols-12">
      <div className="space-y-6 lg:col-span-8">
        {["basket-skeleton-1", "basket-skeleton-2"].map((key) => (
          <BasketItemSkeleton key={key} />
        ))}
      </div>

      <div className="lg:col-span-4">
        <Card className="border-none bg-white p-8 shadow-none dark:bg-gray-800">
          <Skeleton className="mb-6 h-8 w-40" />
          <div className="space-y-4">
            <div className="flex justify-between">
              <Skeleton className="h-4 w-20" />
              <Skeleton className="h-4 w-16" />
            </div>
            <div className="flex justify-between">
              <Skeleton className="h-4 w-20" />
              <Skeleton className="h-4 w-16" />
            </div>
            <Separator />
            <div className="flex justify-between">
              <Skeleton className="h-6 w-16" />
              <Skeleton className="h-6 w-20" />
            </div>
            <Skeleton className="h-12 w-full rounded-full" />
            <Skeleton className="mx-auto h-3 w-48" />
          </div>
        </Card>
      </div>
    </div>
  );
}

export function ConfirmationPageSkeleton() {
  return (
    <div className="mx-auto max-w-3xl">
      <div className="mb-24 space-y-8 text-center">
        <div className="flex justify-center">
          <Skeleton className="size-24 rounded-full" />
        </div>
        <Skeleton className="mx-auto h-16 w-96" />
        <Skeleton className="mx-auto h-8 w-64" />
        <div className="pt-8">
          <Skeleton className="mx-auto mb-2 h-4 w-32" />
          <Skeleton className="mx-auto h-10 w-40" />
        </div>
      </div>

      <div className="border-border mb-24 grid gap-8 border-y py-12 md:grid-cols-3">
        {Array.from({ length: 3 }, (_, idx) => idx).map((i) => (
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

      <Skeleton className="mb-12 h-20 w-full rounded-xl" />

      <div className="flex flex-col items-center justify-center gap-6 sm:flex-row">
        <Skeleton className="h-14 w-64 rounded-full" />
        <Skeleton className="h-14 w-48 rounded-full" />
      </div>
    </div>
  );
}

export function OrdersSkeleton() {
  return (
    <div className="border-border/40 bg-border/40 space-y-px border">
      {Array.from({ length: 5 }, (_, idx) => idx).map((i) => (
        <div
          key={i}
          className="bg-background border-border/40 border-b p-8 last:border-b-0"
        >
          <div className="space-y-4">
            <div className="flex items-center gap-4">
              <Skeleton className="h-8 w-32" />
              <Skeleton className="h-6 w-20" />
            </div>
            <div className="border-border grid grid-cols-2 gap-6 border-t pt-6 md:grid-cols-3">
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

      <div className="border-border/40 overflow-hidden rounded-lg border">
        <div className="border-border/40 border-b px-6 py-4">
          <Skeleton className="h-6 w-32" />
        </div>
        <div className="divide-border/40 divide-y">
          {Array.from({ length: 3 }, (_, idx) => idx).map((i) => (
            <div key={i} className="space-y-3 px-6 py-4">
              <Skeleton className="h-5 w-48" />
              <Skeleton className="h-4 w-32" />
            </div>
          ))}
        </div>
      </div>

      <div className="border-border/40 rounded-lg border p-6">
        <Skeleton className="mb-6 h-6 w-32" />
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
        <Skeleton className="mb-3 h-16 w-64" />
        <Skeleton className="h-6 w-96" />
      </div>

      <div className="grid gap-8 lg:grid-cols-3">
        <div className="space-y-8 lg:col-span-2">
          <div className="border-border/40 rounded-lg border p-6">
            <div className="flex items-center gap-4">
              <Skeleton className="size-16 rounded-full" />
              <div className="flex-1 space-y-2">
                <Skeleton className="h-6 w-32" />
                <Skeleton className="h-4 w-48" />
              </div>
            </div>
          </div>

          <div className="border-border/40 rounded-lg border p-8">
            <div className="mb-6 flex items-center justify-between">
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
            <div className="border-border/40 overflow-hidden rounded-lg border">
              <div className="p-4">
                <Skeleton className="h-6 w-32" />
              </div>
            </div>

            <Skeleton className="h-12 w-full rounded-lg" />
          </div>
        </div>
      </div>
    </div>
  );
}
