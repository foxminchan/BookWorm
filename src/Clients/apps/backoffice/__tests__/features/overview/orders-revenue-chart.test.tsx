import type { PropsWithChildren } from "react";

import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import type { DailyOrders } from "@workspace/types/ordering/dashboard";

import { OrdersRevenueChart } from "@/features/overview/orders-revenue-chart";

vi.mock("recharts", () => ({
  ResponsiveContainer: ({ children }: PropsWithChildren) => (
    <div>{children}</div>
  ),
  LineChart: ({ data }: { data: DailyOrders[] }) => (
    <div data-testid="series">{JSON.stringify(data)}</div>
  ),
  CartesianGrid: () => null,
  Legend: () => null,
  Line: () => null,
  Tooltip: () => null,
  XAxis: () => null,
  YAxis: () => null,
}));

describe("OrdersRevenueChart", () => {
  it("renders loading state", () => {
    render(<OrdersRevenueChart dailyOrders={[]} isLoading={true} />);
    expect(screen.getByText("Loading revenue chart...")).toBeInTheDocument();
  });

  it("passes the server's daily buckets to the chart without changing dates or zero values", () => {
    const dailyOrders = [
      { date: "2026-10-03", orders: 2, revenue: 10 },
      { date: "2026-10-04", orders: 0, revenue: 0 },
    ];
    render(<OrdersRevenueChart dailyOrders={dailyOrders} isLoading={false} />);
    expect(screen.getByTestId("series")).toHaveTextContent(
      JSON.stringify(dailyOrders),
    );
    expect(
      screen.getByText("Daily orders and completed-order revenue (UTC)"),
    ).toBeInTheDocument();
  });
});
