import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { KPICards } from "@/features/overview/kpi-cards";

const totals = {
  totalOrders: 40,
  totalRevenue: 250,
  totalCustomers: 30,
  totalBooks: 35,
};

describe("KPICards", () => {
  it("renders loading statistics", () => {
    render(<KPICards {...totals} isLoading={true} />);
    expect(screen.getByText("Loading statistics...")).toBeInTheDocument();
  });

  it("displays the backend totals and the registered-customer label", () => {
    render(<KPICards {...totals} isLoading={false} />);
    for (const value of [
      "$250.00",
      "40",
      "30",
      "35",
      "40 orders",
      "Registered Customers",
    ]) {
      expect(screen.getByText(value)).toBeInTheDocument();
    }
  });

  it("renders an empty store", () => {
    render(
      <KPICards
        totalOrders={0}
        totalRevenue={0}
        totalCustomers={0}
        totalBooks={0}
        isLoading={false}
      />,
    );
    expect(screen.getByText("$0.00")).toBeInTheDocument();
    expect(screen.getAllByText("0")).toHaveLength(3);
  });
});
