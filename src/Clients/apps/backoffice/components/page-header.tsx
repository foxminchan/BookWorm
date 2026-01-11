import type React from "react";

import {
  Breadcrumb,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@workspace/ui/components/breadcrumb";

type PageHeaderProps = {
  title: string;
  description?: string;
  breadcrumbs: { label: string; href?: string; isActive?: boolean }[];
  action?: React.ReactNode;
};

export function PageHeader({
  title,
  description,
  breadcrumbs,
  action,
}: PageHeaderProps) {
  return (
    <div className="space-y-6">
      <nav aria-label="Breadcrumb">
        <Breadcrumb>
          <BreadcrumbList>
            {breadcrumbs.map((item, index) => (
              <div key={index} className="flex items-center gap-2">
                {index > 0 && <BreadcrumbSeparator />}
                {item.isActive || !item.href ? (
                  <BreadcrumbPage>{item.label}</BreadcrumbPage>
                ) : (
                  <BreadcrumbLink href={item.href}>{item.label}</BreadcrumbLink>
                )}
              </div>
            ))}
          </BreadcrumbList>
        </Breadcrumb>
      </nav>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-foreground text-3xl font-bold">{title}</h1>
          {description && (
            <p className="text-muted-foreground mt-1">{description}</p>
          )}
        </div>
        {action && <div>{action}</div>}
      </div>
    </div>
  );
}
