import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { PageContainer } from "@/components/page-container";
import { APP_CONFIG } from "@/config/app-config";
import { CustomersTable } from "@/features/customers/table/table";

export const metadata: Metadata = createMetadata({
  title: "Customers",
  description: "View and manage your customers",
  appName: APP_CONFIG.name,
});

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Customers", isActive: true },
];

export default function CustomersPage() {
  return (
    <PageContainer
      title="Customers Management"
      description="View and manage your customers"
      breadcrumbs={breadcrumbs}
    >
      <CustomersTable />
    </PageContainer>
  );
}
