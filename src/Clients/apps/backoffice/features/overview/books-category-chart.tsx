"use client";

import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from "recharts";

import type { Book } from "@workspace/types/catalog/books";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";

import { BooksCategoryChartSkeleton } from "@/components/loading-skeleton";
import { CHART_COLORS, CHART_THEME } from "@/lib/constants";

type BooksCategoryChartProps = {
  books: Book[];
  isLoading: boolean;
};

export function BooksCategoryChart({
  books,
  isLoading,
}: BooksCategoryChartProps) {
  if (isLoading) {
    return <BooksCategoryChartSkeleton />;
  }

  const categoryStats = books.reduce(
    (acc: Array<{ name: string; value: number }>, book: Book) => {
      const categoryName = book.category?.name || "Other";
      const existing = acc.find((c) => c.name === categoryName);
      if (existing) {
        existing.value += 1;
      } else {
        acc.push({ name: categoryName, value: 1 });
      }
      return acc;
    },
    [],
  );

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
            >
              {categoryStats.map((entry, index) => (
                <Cell
                  key={`cell-${index}`}
                  fill={CHART_COLORS[index % CHART_COLORS.length]}
                />
              ))}
            </Pie>
            <Tooltip contentStyle={CHART_THEME.tooltip} />
          </PieChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
