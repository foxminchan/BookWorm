import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import OrdersPageClient from "@/features/ordering/orders/orders-page-client";

export const metadata: Metadata = createMetadata({
  title: "Order History",
  description: "Track and inspect your past BookWorm orders.",
});

export default function OrdersPage() {
  return <OrdersPageClient />;
}
