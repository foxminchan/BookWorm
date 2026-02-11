import type { Publisher } from "@workspace/types/catalog/publishers";

import { PublisherCardSkeleton } from "@/components/loading-skeleton";

import PublisherCard from "./publisher-card";

const SKELETON_KEYS = Array.from({ length: 9 }, (_, i) => `skeleton-${i}`);

type PublishersGridProps = {
  publishers: Publisher[];
  isLoading: boolean;
};

export default function PublishersGrid({
  publishers,
  isLoading,
}: Readonly<PublishersGridProps>) {
  if (isLoading) {
    return (
      <div className="mb-20 grid grid-cols-2 gap-6 md:grid-cols-3">
        {SKELETON_KEYS.map((key) => (
          <PublisherCardSkeleton key={key} />
        ))}
      </div>
    );
  }

  return (
    <div className="mb-20 grid grid-cols-2 gap-6 md:grid-cols-3">
      {publishers.map((publisher) => (
        <PublisherCard
          key={publisher.id}
          id={publisher.id}
          name={publisher.name ?? "Unknown Publisher"}
        />
      ))}
    </div>
  );
}
