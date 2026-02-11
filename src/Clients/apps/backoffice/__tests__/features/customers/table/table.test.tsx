import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createMockCustomer } from "@/__tests__/factories";
import { renderWithProviders } from "@/__tests__/utils/test-utils";
import { CustomersTable } from "@/features/customers/table/table";

// Mock the data hook
const mockUseBuyers = vi.hoisted(() => vi.fn());
vi.mock("@workspace/api-hooks/ordering/buyers/useBuyers", () => ({
  default: mockUseBuyers,
}));

// Mock the delete hook used by CellAction
vi.mock("@workspace/api-hooks/ordering/buyers/useDeleteBuyer", () => ({
  default: () => ({ mutate: vi.fn(), isPending: false }),
}));

describe("CustomersTable", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render loading state", () => {
    mockUseBuyers.mockReturnValue({
      data: undefined,
      isLoading: true,
    });

    renderWithProviders(<CustomersTable />);

    expect(screen.getByRole("status")).toBeInTheDocument();
  });

  it("should render customer data in table", () => {
    const customers = [
      createMockCustomer({ name: "John Doe" }),
      createMockCustomer({ name: "Jane Smith" }),
    ];

    mockUseBuyers.mockReturnValue({
      data: { items: customers, totalCount: 2 },
      isLoading: false,
    });

    renderWithProviders(<CustomersTable />);

    expect(screen.getByText("John Doe")).toBeInTheDocument();
    expect(screen.getByText("Jane Smith")).toBeInTheDocument();
  });

  it("should display total count in description", () => {
    mockUseBuyers.mockReturnValue({
      data: { items: [], totalCount: 15 },
      isLoading: false,
    });

    renderWithProviders(<CustomersTable />);

    expect(
      screen.getAllByText("Total: 15 customers").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("should render table title", () => {
    mockUseBuyers.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    });

    renderWithProviders(<CustomersTable />);

    expect(screen.getByText("All Customers")).toBeInTheDocument();
  });

  it("should handle empty data gracefully", () => {
    mockUseBuyers.mockReturnValue({
      data: undefined,
      isLoading: false,
    });

    renderWithProviders(<CustomersTable />);

    expect(
      screen.getAllByText("Total: 0 customers").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("should pass pagination params to the hook", () => {
    mockUseBuyers.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    });

    renderWithProviders(<CustomersTable />);

    expect(mockUseBuyers).toHaveBeenCalledWith(
      expect.objectContaining({
        pageIndex: 1,
        pageSize: 10,
      }),
    );
  });
});
