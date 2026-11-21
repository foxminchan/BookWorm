import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { APP_CONFIG } from "@/config/app-config";
import { PublishersListing } from "@/features/catalog/publishers-listing";

export const metadata: Metadata = createMetadata({
  title: "Publishers",
  description: "Manage book publishers",
  appName: APP_CONFIG.name,
});

export default function PublishersPage() {
  return <PublishersListing />;
}
