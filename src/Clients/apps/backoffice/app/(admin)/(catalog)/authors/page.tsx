import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { APP_CONFIG } from "@/config/app-config";
import { AuthorsListing } from "@/features/catalog/authors-listing";

export const metadata: Metadata = createMetadata({
  title: "Authors",
  description: "Manage book authors",
  appName: APP_CONFIG.name,
});

export default function AuthorsPage() {
  return <AuthorsListing />;
}
