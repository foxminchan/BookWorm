import type { PropsWithChildren } from "react";

import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import type { CategoryCount } from "@workspace/types/ordering/dashboard";

import { BooksCategoryChart } from "@/features/overview/books-category-chart";

vi.mock("recharts", () => ({
  ResponsiveContainer: ({ children }: PropsWithChildren) => (
    <div>{children}</div>
  ),
  PieChart: ({ children }: PropsWithChildren) => <div>{children}</div>,
  Pie: ({ data }: { data: CategoryCount[] }) => (
    <div data-testid="categories">{JSON.stringify(data)}</div>
  ),
  Sector: () => null,
  Tooltip: () => null,
}));

describe("BooksCategoryChart", () => {
  it("renders loading state", () => {
    render(<BooksCategoryChart categories={[]} isLoading={true} />);
    expect(screen.getByText("Loading category chart...")).toBeInTheDocument();
  });

  it("renders global category counts from the server, including uncategorized books", () => {
    const categories = [
      { name: "Fiction", value: 30 },
      { name: "Other", value: 5 },
    ];
    render(<BooksCategoryChart categories={categories} isLoading={false} />);
    expect(screen.getByTestId("categories")).toHaveTextContent(
      JSON.stringify(categories),
    );
    expect(screen.getByText("Books by Category")).toBeInTheDocument();
  });
});
