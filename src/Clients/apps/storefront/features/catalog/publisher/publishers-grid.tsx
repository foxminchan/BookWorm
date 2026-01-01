"use client";

import PublisherCard from "./publisher-card";
import { PublisherCardSkeleton } from "@/components/loading-skeleton";

type Publisher = {
  id: string;
  name: string | null;
};

type PublishersGridProps = {
  publishers: Publisher[];
  isLoading: boolean;
};

export default function PublishersGrid({
  publishers,
  isLoading,
}: PublishersGridProps) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-2 md:grid-cols-3 gap-6 mb-20">
        {Array.from({ length: 9 }).map((_, i) => (
          <PublisherCardSkeleton key={i} />
        ))}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-2 md:grid-cols-3 gap-6 mb-20">
      {publishers.map((publisher) => (
        <PublisherCard
          key={publisher.id}
          id={publisher.id}
          name={publisher.name || "Unknown Publisher"}
        />
      ))}
    </div>
  );
}
