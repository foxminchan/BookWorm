"use client";

import { PublisherCardSkeleton } from "@/components/loading-skeleton";

import PublisherCard from "./publisher-card";

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
      <div className="mb-20 grid grid-cols-2 gap-6 md:grid-cols-3">
        {Array.from({ length: 9 }).map((_, i) => (
          <PublisherCardSkeleton key={i} />
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
          name={publisher.name || "Unknown Publisher"}
        />
      ))}
    </div>
  );
}
