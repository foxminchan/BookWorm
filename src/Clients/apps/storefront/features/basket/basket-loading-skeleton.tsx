import { Card } from "@workspace/ui/components/card";
import { Separator } from "@workspace/ui/components/separator";
import { Skeleton } from "@workspace/ui/components/skeleton";

import { BasketItemSkeleton } from "@/components/loading-skeleton";

export default function BasketLoadingSkeleton() {
  return (
    <div className="mx-auto grid max-w-5xl grid-cols-1 gap-12 lg:grid-cols-12">
      <div className="space-y-6 lg:col-span-8">
        {[1, 2].map((i) => (
          <BasketItemSkeleton key={i} />
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
