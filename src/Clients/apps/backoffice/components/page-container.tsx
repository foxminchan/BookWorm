import type React from "react";

import { PageHeader } from "@/components/page-header";

type BreadcrumbItem = {
  label: string;
  href?: string;
  isActive?: boolean;
};

type PageContainerProps = Readonly<{
  title: string;
  description?: string;
  breadcrumbs: BreadcrumbItem[];
  action?: React.ReactNode;
  children: React.ReactNode;
}>;

/**
 * Standardized page wrapper combining breadcrumbs, header, action slot, and content spacing.
 */
export function PageContainer({
  title,
  description,
  breadcrumbs,
  action,
  children,
}: PageContainerProps) {
  return (
    <div className="space-y-6">
      <PageHeader
        title={title}
        description={description}
        breadcrumbs={breadcrumbs}
        action={action}
      />
      {children}
    </div>
  );
}
