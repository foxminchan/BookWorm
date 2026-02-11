"use client";

import { useCallback, useMemo } from "react";

import { Pie, PieChart, ResponsiveContainer, Sector, Tooltip } from "recharts";
import type { PieSectorShapeProps } from "recharts/types/polar/Pie";

import type { Book } from "@workspace/types/catalog/books";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";

import { BooksCategoryChartSkeleton } from "@/components/loading-skeleton";
import { CHART_COLORS, CHART_THEME } from "@/lib/constants";

type BooksCategoryChartProps = Readonly<{
  books: Book[];
  isLoading: boolean;
}>;

export function BooksCategoryChart({
  books,
  isLoading,
}: BooksCategoryChartProps) {
  const categoryStats = useMemo(() => {
    const categoryMap = new Map<string, number>();
    for (const book of books) {
      const categoryName = book.category?.name ?? "Other";
      categoryMap.set(categoryName, (categoryMap.get(categoryName) ?? 0) + 1);
    }
    return Array.from(categoryMap.entries()).map(([name, value]) => ({
      name,
      value,
    }));
  }, [books]);

  const renderSector = useCallback(
    (props: PieSectorShapeProps, index: number) => (
      <Sector {...props} fill={CHART_COLORS[index % CHART_COLORS.length]} />
    ),
    [],
  );

  if (isLoading) {
    return <BooksCategoryChartSkeleton />;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Books by Category</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <PieChart>
            <Pie
              data={categoryStats}
              cx="50%"
              cy="50%"
              labelLine={false}
              label={({ name, value }) => `${name}: ${value}`}
              outerRadius={80}
              fill="#8884d8"
              dataKey="value"
              shape={renderSector}
            />
            <Tooltip contentStyle={CHART_THEME.tooltip} />
          </PieChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
