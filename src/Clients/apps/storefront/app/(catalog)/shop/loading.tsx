import { BookCardSkeleton } from "@/components/loading-skeleton";

export default function ShopLoading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="grid grid-cols-2 gap-6 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
        {Array.from({ length: 10 }, () => crypto.randomUUID()).map((key) => (
          <BookCardSkeleton key={key} />
        ))}
      </div>
    </div>
  );
}
