import { BasketItemSkeleton } from "@/components/loading-skeleton";

export default function BasketLoading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="space-y-4">
        {["first", "second", "third"].map((id) => (
          <BasketItemSkeleton key={id} />
        ))}
      </div>
    </div>
  );
}
