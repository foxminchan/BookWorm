import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { APP_CONFIG } from "@/config/app-config";
import { OrderDetailView } from "@/features/orders/order-detail-view";

export const metadata: Metadata = createMetadata({
  title: "Order Details",
  description: "Inspect order summary, line items, and event history",
  appName: APP_CONFIG.name,
});

type OrderDetailPageProps = {
  params: Promise<{ id: string }>;
};

export default async function OrderDetailPage({
  params,
}: Readonly<OrderDetailPageProps>) {
  const { id } = await params;

  return <OrderDetailView orderId={id} />;
}
