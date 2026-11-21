import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import AccountPageClient from "@/features/account/account-page-client";

export const metadata: Metadata = createMetadata({
  title: "Your Account",
  description:
    "Manage your profile information, delivery address, and order history.",
});

export default function AccountPage() {
  return <AccountPageClient />;
}
