import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { APP_CONFIG } from "@/config/app-config";
import { CategoriesListing } from "@/features/catalog/categories-listing";

export const metadata: Metadata = createMetadata({
  title: "Categories",
  description: "Manage book categories",
  appName: APP_CONFIG.name,
});

export default function CategoriesPage() {
  return <CategoriesListing />;
}
