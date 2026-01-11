"use client";

import { PageHeader } from "@/components/page-header";
import { CustomersTable } from "@/features/customers/table/table";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Customers", isActive: true },
];

export default function CustomersPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Customers Management"
        description="View and manage your customers"
        breadcrumbs={breadcrumbs}
      />
      <CustomersTable />
    </div>
  );
}
