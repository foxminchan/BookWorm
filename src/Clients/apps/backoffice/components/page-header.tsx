import type React from "react";
import { Fragment } from "react";

import {
  Breadcrumb,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@workspace/ui/components/breadcrumb";

type BreadcrumbItem = {
  label: string;
  href?: string;
  isActive?: boolean;
};

type PageHeaderProps = {
  title: string;
  description?: string;
  breadcrumbs: BreadcrumbItem[];
  action?: React.ReactNode;
};

export function PageHeader({
  title,
  description,
  breadcrumbs,
  action,
}: Readonly<PageHeaderProps>) {
  return (
    <div className="space-y-6">
      <nav aria-label="Breadcrumb">
        <Breadcrumb>
          <BreadcrumbList>
            {breadcrumbs.map((item, index) => (
              <Fragment key={item.label}>
                {index > 0 && <BreadcrumbSeparator />}
                {item.isActive || !item.href ? (
                  <BreadcrumbPage>{item.label}</BreadcrumbPage>
                ) : (
                  <BreadcrumbLink href={item.href}>{item.label}</BreadcrumbLink>
                )}
              </Fragment>
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
