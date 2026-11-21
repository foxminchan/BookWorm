import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import OrderDetailPageClient from "@/features/ordering/order-detail/order-detail-page-client";

export const metadata: Metadata = createMetadata({
  title: "Order Details",
  description: "View your order status, items, and delivery timeline.",
});

export default async function OrderDetailPage({
  params,
}: Readonly<{
  params: Promise<{ id: string }>;
}>) {
  const { id } = await params;

  return <OrderDetailPageClient orderId={id} />;
}
