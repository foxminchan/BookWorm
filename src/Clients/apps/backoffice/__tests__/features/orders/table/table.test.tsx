import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createMockOrder } from "@/__tests__/factories";
import { renderWithProviders } from "@/__tests__/utils/test-utils";
import { OrdersTable } from "@/features/orders/table/table";

// Mock the data hook
const mockUseOrders = vi.hoisted(() => vi.fn());
vi.mock("@workspace/api-hooks/ordering/orders/useOrders", () => ({
  default: mockUseOrders,
}));

// Mock the mutation hooks used by CellAction
vi.mock("@workspace/api-hooks/ordering/orders/useCompleteOrder", () => ({
  default: () => ({ mutate: vi.fn(), isPending: false }),
}));

vi.mock("@workspace/api-hooks/ordering/orders/useCancelOrder", () => ({
  default: () => ({ mutate: vi.fn(), isPending: false }),
}));

vi.mock("@workspace/api-hooks/ordering/orders/useDeleteOrder", () => ({
  default: () => ({ mutate: vi.fn(), isPending: false }),
}));

describe("OrdersTable", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render loading state", () => {
    mockUseOrders.mockReturnValue({
      data: undefined,
      isLoading: true,
    });

    renderWithProviders(<OrdersTable />);

    expect(screen.getByRole("status")).toBeInTheDocument();
  });

  it("should render order data in table", () => {
    const orders = [
      createMockOrder({ status: "New" }),
      createMockOrder({ status: "Completed" }),
    ];

    mockUseOrders.mockReturnValue({
      data: { items: orders, totalCount: 2 },
      isLoading: false,
    });

    renderWithProviders(<OrdersTable />);

    expect(screen.getByText("New")).toBeInTheDocument();
    expect(screen.getByText("Completed")).toBeInTheDocument();
  });

  it("should display total count in description", () => {
    mockUseOrders.mockReturnValue({
      data: { items: [], totalCount: 7 },
      isLoading: false,
    });

    renderWithProviders(<OrdersTable />);

    expect(
      screen.getAllByText("Total: 7 orders").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("should render table title", () => {
    mockUseOrders.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    });

    renderWithProviders(<OrdersTable />);

    expect(screen.getByText("All Orders")).toBeInTheDocument();
  });

  it("should handle empty data gracefully", () => {
    mockUseOrders.mockReturnValue({
      data: undefined,
      isLoading: false,
    });

    renderWithProviders(<OrdersTable />);

    expect(
      screen.getAllByText("Total: 0 orders").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("should pass status filter to the hook", () => {
    mockUseOrders.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    });

    renderWithProviders(<OrdersTable statusFilter="Completed" />);

    expect(mockUseOrders).toHaveBeenCalledWith(
      expect.objectContaining({
        status: "Completed",
        pageIndex: 1,
        pageSize: 10,
      }),
    );
  });

  it("should not include status in query when no filter is provided", () => {
    mockUseOrders.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    });

    renderWithProviders(<OrdersTable />);

    const callArgs = mockUseOrders.mock.calls[0]?.[0];
    expect(callArgs).not.toHaveProperty("status");
  });
});
