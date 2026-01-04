import { BasketItemSkeleton } from "@/components/loading-skeleton";

export default function BasketLoading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="space-y-4">
        {Array.from({ length: 3 }).map((_, i) => (
          <BasketItemSkeleton key={i} />
        ))}
      </div>
    </div>
  );
}
