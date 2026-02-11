import { PublisherCardSkeleton } from "@/components/loading-skeleton";

export default function PublishersLoading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 9 }, () => crypto.randomUUID()).map((id) => (
          <PublisherCardSkeleton key={id} />
        ))}
      </div>
    </div>
  );
}
