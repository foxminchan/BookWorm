"use client";

import { useCallback } from "react";

import { Pie, PieChart, ResponsiveContainer, Sector, Tooltip } from "recharts";
import type { PieSectorShapeProps } from "recharts/types/polar/Pie";

import type { CategoryCount } from "@workspace/types/ordering/dashboard";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";

import { BooksCategoryChartSkeleton } from "@/components/loading-skeleton";
import { CHART_COLORS, CHART_THEME } from "@/lib/constants";

type BooksCategoryChartProps = Readonly<{
  categories: CategoryCount[];
  isLoading: boolean;
}>;

export function BooksCategoryChart({
  categories,
  isLoading,
}: BooksCategoryChartProps) {
  const renderSector = useCallback(
    (props: PieSectorShapeProps, index?: string | number) => {
      const i = typeof index === "number" ? index : 0;
      return <Sector {...props} fill={CHART_COLORS[i % CHART_COLORS.length]} />;
    },
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
              data={categories}
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
