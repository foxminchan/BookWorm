import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { APP_CONFIG } from "@/config/app-config";
import { ReviewsListing } from "@/features/reviews/reviews-listing";

export const metadata: Metadata = createMetadata({
  title: "Reviews",
  description: "Moderate customer reviews for your books",
  appName: APP_CONFIG.name,
});

export default function ReviewsPage() {
  return <ReviewsListing />;
}
