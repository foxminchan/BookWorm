import type { ComponentProps, ReactNode } from "react";

import { Card, CardContent, CardHeader } from "@workspace/ui/components/card";
import { Skeleton } from "@workspace/ui/components/skeleton";

type SkeletonWrapperProps = Readonly<{
  label: string;
  as?: "div" | typeof Card;
  className?: string;
  children: ReactNode;
}> &
  ComponentProps<"div">;

/** Generate stable string keys for static skeleton lists that never reorder. */
function keys(count: number, prefix: string): string[] {
  return Array.from({ length: count }, (_, i) => `${prefix}-${i}`);
}

function SkeletonWrapper({
  label,
  as: Wrapper = "div",
  className,
  children,
  ...rest
}: SkeletonWrapperProps) {
  return (
    <Wrapper
      className={className}
      role="status"
      aria-live="polite"
      aria-busy="true"
      {...rest}
    >
      <span className="sr-only">{label}</span>
      {children}
    </Wrapper>
  );
}

export function OrderDetailSkeleton() {
  return (
    <SkeletonWrapper label="Loading order details..." as={Card}>
      <CardContent className="space-y-4 pt-6">
        <Skeleton className="h-12 w-full" />
        <Skeleton className="h-12 w-full" />
        <Skeleton className="h-12 w-full" />
      </CardContent>
    </SkeletonWrapper>
  );
}

export function BookFormSkeleton() {
  return (
    <SkeletonWrapper label="Loading book form..." className="space-y-6">
      <Card>
        <CardHeader>
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-4 w-64" />
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-10 w-full" />
          </div>
          <div className="space-y-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-24 w-full" />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Skeleton className="h-4 w-16" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div className="space-y-2">
              <Skeleton className="h-4 w-32" />
              <Skeleton className="h-10 w-full" />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <Skeleton className="h-8 w-40" />
          <Skeleton className="h-4 w-56" />
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Skeleton className="h-4 w-20" />
            <Skeleton className="h-10 w-full" />
          </div>
          <div className="space-y-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-10 w-full" />
          </div>
          <div className="space-y-2">
            <Skeleton className="h-4 w-28" />
            <Skeleton className="h-10 w-full" />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <Skeleton className="h-8 w-32" />
          <Skeleton className="h-4 w-48" />
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Skeleton className="h-4 w-20" />
            <Skeleton className="h-10 w-full" />
          </div>
          <div className="space-y-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-10 w-full" />
          </div>
        </CardContent>
      </Card>

      <div className="flex justify-end gap-4">
        <Skeleton className="h-10 w-24" />
        <Skeleton className="h-10 w-32" />
      </div>
    </SkeletonWrapper>
  );
}

export function TableSkeleton({ rows = 5 }: Readonly<{ rows?: number }>) {
  return (
    <SkeletonWrapper label="Loading table data..." className="space-y-4">
      <div className="flex items-center justify-between">
        <Skeleton className="h-10 w-64" />
        <Skeleton className="h-10 w-32" />
      </div>
      <div className="space-y-2">
        <Skeleton className="h-12 w-full" />
        {keys(rows, "table-row").map((key) => (
          <Skeleton key={key} className="h-16 w-full" />
        ))}
      </div>
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-10 w-64" />
      </div>
    </SkeletonWrapper>
  );
}

export function CardSkeleton() {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-6 w-32" />
        <Skeleton className="h-4 w-48" />
      </CardHeader>
      <CardContent>
        <Skeleton className="h-24 w-full" />
      </CardContent>
    </Card>
  );
}

export function StatCardsSkeleton() {
  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      {keys(4, "stat-card").map((key) => (
        <Card key={key}>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-4 w-4" />
          </CardHeader>
          <CardContent>
            <Skeleton className="h-8 w-20" />
            <Skeleton className="h-3 w-32" />
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

export function SimpleTableSkeleton({
  rows = 5,
  columns = 2,
}: Readonly<{
  rows?: number;
  columns?: number;
}>) {
  return (
    <SkeletonWrapper label="Loading table..." as={Card}>
      <CardHeader>
        <Skeleton className="h-6 w-48" />
        <Skeleton className="h-4 w-64" />
      </CardHeader>
      <CardContent>
        <div className="rounded-lg border">
          <div className="space-y-2 p-4">
            <Skeleton className="h-10 w-full" />
            {keys(rows, "simple-row").map((rowKey) => (
              <div key={rowKey} className="flex gap-4">
                {keys(columns, `${rowKey}-col`).map((colKey) => (
                  <Skeleton key={colKey} className="h-8 flex-1" />
                ))}
              </div>
            ))}
          </div>
        </div>
      </CardContent>
    </SkeletonWrapper>
  );
}

export function FilterTableSkeleton({
  description,
  rows = 10,
  columns = 4,
}: Readonly<{
  description?: string;
  rows?: number;
  columns?: number;
}>) {
  return (
    <SkeletonWrapper label="Loading filtered table..." as={Card}>
      <CardHeader>
        <Skeleton className="h-7 w-48" />
        {description && <Skeleton className="h-4 w-64" />}
      </CardHeader>
      <CardContent>
        <div className="rounded-md border">
          <div className="space-y-2 p-4">
            <Skeleton className="h-10 w-full" />
            {keys(rows, "filter-row").map((rowKey) => (
              <div key={rowKey} className="flex gap-4">
                {keys(columns, `${rowKey}-col`).map((colKey) => (
                  <Skeleton key={colKey} className="h-4 flex-1" />
                ))}
              </div>
            ))}
          </div>
        </div>
      </CardContent>
    </SkeletonWrapper>
  );
}

export function RecentOrdersTableSkeleton() {
  return (
    <SkeletonWrapper label="Loading recent orders..." as={Card}>
      <CardHeader>
        <Skeleton className="h-6 w-32" />
        <Skeleton className="h-4 w-40" />
      </CardHeader>
      <CardContent>
        <div className="space-y-2">
          <div className="flex gap-4">
            {keys(4, "orders-header").map((key) => (
              <Skeleton key={key} className="h-10 flex-1" />
            ))}
          </div>
          {keys(5, "orders-row").map((rowKey) => (
            <div key={rowKey} className="flex gap-4">
              {keys(4, `${rowKey}-col`).map((colKey) => (
                <Skeleton key={colKey} className="h-8 flex-1" />
              ))}
            </div>
          ))}
        </div>
      </CardContent>
    </SkeletonWrapper>
  );
}

export function OrdersRevenueChartSkeleton() {
  return (
    <SkeletonWrapper
      label="Loading revenue chart..."
      as={Card}
      className="lg:col-span-2"
    >
      <CardHeader>
        <Skeleton className="h-6 w-48" />
        <Skeleton className="h-4 w-32" />
      </CardHeader>
      <CardContent>
        <Skeleton className="h-75 w-full" />
      </CardContent>
    </SkeletonWrapper>
  );
}

export function KPICardsSkeleton() {
  return (
    <SkeletonWrapper
      label="Loading statistics..."
      className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4"
    >
      {keys(4, "kpi-card").map((key) => (
        <Card key={key}>
          <CardHeader className="pb-2">
            <Skeleton className="h-4 w-24" />
          </CardHeader>
          <CardContent>
            <Skeleton className="h-8 w-20" />
            <Skeleton className="mt-2 h-3 w-16" />
          </CardContent>
        </Card>
      ))}
    </SkeletonWrapper>
  );
}

export function BooksCategoryChartSkeleton() {
  return (
    <SkeletonWrapper label="Loading category chart..." as={Card}>
      <CardHeader>
        <Skeleton className="h-6 w-40" />
      </CardHeader>
      <CardContent>
        <Skeleton className="h-75 w-full rounded-full" />
      </CardContent>
    </SkeletonWrapper>
  );
}

export function ClassificationSkeleton() {
  return (
    <SkeletonWrapper label="Loading classification options...">
      <div className="space-y-2">
        <Skeleton className="h-4 w-20" />
        <Skeleton className="h-10 w-full" />
      </div>
      <div className="space-y-2">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-10 w-full" />
      </div>
      <div className="space-y-2">
        <Skeleton className="h-4 w-16" />
        <div className="space-y-2">
          {keys(5, "classification").map((key) => (
            <Skeleton key={key} className="h-6 w-full" />
          ))}
        </div>
      </div>
    </SkeletonWrapper>
  );
}
