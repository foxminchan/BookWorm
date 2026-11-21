import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { APP_CONFIG } from "@/config/app-config";
import { OrdersListing } from "@/features/orders/orders-listing";

export const metadata: Metadata = createMetadata({
  title: "Orders",
  description: "Manage and track all customer orders",
  appName: APP_CONFIG.name,
});

export default function OrdersPage() {
  return <OrdersListing />;
}
