import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import BasketPageClient from "@/features/basket/basket-page-client";

export const metadata: Metadata = createMetadata({
  title: "Your Basket",
  description: "Review and manage books in your shopping basket.",
});

export default function BasketPage() {
  return <BasketPageClient />;
}
