import { Card } from "@workspace/ui/components/card";
import { Separator } from "@workspace/ui/components/separator";
import { Skeleton } from "@workspace/ui/components/skeleton";
import { BasketItemSkeleton } from "@/components/loading-skeleton";

export default function BasketLoadingSkeleton() {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 max-w-5xl mx-auto">
      <div className="lg:col-span-8 space-y-6">
        {[1, 2].map((i) => (
          <BasketItemSkeleton key={i} />
        ))}
      </div>

      <div className="lg:col-span-4">
        <Card className="border-none shadow-none bg-white dark:bg-gray-800 p-8">
          <Skeleton className="h-8 w-40 mb-6" />
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
            <Skeleton className="h-3 w-48 mx-auto" />
          </div>
        </Card>
      </div>
    </div>
  );
}
