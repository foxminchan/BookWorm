import { PublisherCardSkeleton } from "@/components/loading-skeleton";
import PublishersHeader from "@/features/catalog/publisher/publishers-header";

export default function PublishersLoading() {
  return (
    <div className="container mx-auto grow px-4 py-16 md:py-24">
      <PublishersHeader />
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 9 }, (_, idx) => (
          <PublisherCardSkeleton key={idx} />
        ))}
      </div>
    </div>
  );
}
