"use client";

import { Building2 } from "lucide-react";

import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";

import { EmptyState } from "@/components/empty-state";
import PublishersGrid from "@/features/catalog/publisher/publishers-grid";
import PublishersHeader from "@/features/catalog/publisher/publishers-header";
import PublishersInfoSection from "@/features/catalog/publisher/publishers-info-section";

export default function PublishersPage() {
  const { data: publishers, isLoading } = usePublishers();
  const publisherItems = publishers ?? [];

  return (
    <main className="container mx-auto grow px-4 py-16 md:py-24">
      {!isLoading && publisherItems.length === 0 ? (
        <div className="flex min-h-[60vh] items-center justify-center">
          <EmptyState
            icon={Building2}
            title="Coming Soon"
            description="We're still discovering amazing publishers to bring to BookWorm. Check back soon for an exciting collection."
            actionLabel="Return to Home"
            actionHref="/"
          />
        </div>
      ) : (
        <>
          <PublishersHeader />
          <PublishersGrid publishers={publisherItems} isLoading={isLoading} />
          <PublishersInfoSection />
        </>
      )}
    </main>
  );
}
