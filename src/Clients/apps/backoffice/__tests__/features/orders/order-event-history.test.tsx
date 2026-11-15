import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { OrderEventHistory } from "@/features/orders/order-event-history";

const mockUseOrderHistory = vi.fn();
const mockUseOrderSummary = vi.fn();

vi.mock("@workspace/api-hooks/ordering/orders/useOrderHistory", () => ({
  default: (...args: unknown[]) => mockUseOrderHistory(...args),
}));

vi.mock("@workspace/api-hooks/ordering/orders/useOrderSummary", () => ({
  default: (...args: unknown[]) => mockUseOrderSummary(...args),
}));

describe("OrderEventHistory", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders event history and allows replaying to a specific version", async () => {
    const user = userEvent.setup();
    mockUseOrderHistory.mockReturnValue({
      data: [
        {
          sequence: 1,
          version: 1,
          eventType: "order_placed",
          timestamp: "2026-11-15T10:00:00Z",
          status: "New",
          totalPrice: 150,
        },
        {
          sequence: 2,
          version: 2,
          eventType: "order_completed",
          timestamp: "2026-11-15T11:00:00Z",
          status: "Completed",
          totalPrice: 150,
        },
      ],
      isLoading: false,
      isError: false,
    });

    mockUseOrderSummary.mockImplementation((_id, query) => ({
      data: {
        id: "order-1",
        status: query?.version === 1 ? "New" : "Completed",
        totalPrice: 150,
        version: query?.version ?? 2,
      },
    }));

    render(<OrderEventHistory orderId="order-1" />);

    expect(screen.getByText("order_placed")).toBeInTheDocument();
    expect(screen.getByText("order_completed")).toBeInTheDocument();
    expect(screen.getByText("Current stream state (v2):")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Replay v1" }));

    expect(screen.getByText("Replayed at v1:")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Latest/i })).toBeInTheDocument();
  });

  it("renders empty message when no event stream exists", () => {
    mockUseOrderHistory.mockReturnValue({
      data: [],
      isLoading: false,
      isError: false,
    });
    mockUseOrderSummary.mockReturnValue({ data: undefined });

    render(<OrderEventHistory orderId="order-1" />);

    expect(
      screen.getByText("No event stream recorded for this order yet."),
    ).toBeInTheDocument();
  });
});
