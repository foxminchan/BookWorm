"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { EmptyState } from "@/components/empty-state";
import { Building2 } from "lucide-react";
import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";
import {
  PublishersHeader,
  PublishersGrid,
  PublishersInfoSection,
} from "@/features/publisher";

export default function PublishersPage() {
  const { data: publishers, isLoading } = usePublishers();
  const publisherItems = publishers ?? [];

  return (
    <div className="min-h-screen flex flex-col bg-background">
      <Header />

      <main className="grow container mx-auto px-4 py-16 md:py-24">
        {!isLoading && publisherItems.length === 0 ? (
          <div className="min-h-[60vh] flex items-center justify-center">
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

      <Footer />
    </div>
  );
}
