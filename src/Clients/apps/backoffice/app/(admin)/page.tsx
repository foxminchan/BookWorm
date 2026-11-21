import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { APP_CONFIG } from "@/config/app-config";
import { OverviewDashboard } from "@/features/overview/overview-dashboard";

export const metadata: Metadata = createMetadata({
  title: "Overview",
  description: APP_CONFIG.description,
  appName: APP_CONFIG.name,
});

export default function OverviewPage() {
  return <OverviewDashboard />;
}
