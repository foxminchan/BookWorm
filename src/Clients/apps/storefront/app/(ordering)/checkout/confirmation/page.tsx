import { Suspense } from "react";

import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { ConfirmationPageSkeleton } from "@/components/loading-skeleton";
import ConfirmationPageClient from "@/features/ordering/checkout/confirmation-page-client";

export const metadata: Metadata = createMetadata({
  title: "Order Confirmation",
  description: "Thank you for your BookWorm order.",
});

export default function ConfirmationPage() {
  return (
    <Suspense
      fallback={
        <main className="container mx-auto grow px-4 py-24">
          <ConfirmationPageSkeleton />
        </main>
      }
    >
      <ConfirmationPageClient />
    </Suspense>
  );
}
