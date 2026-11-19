import { screen } from "@testing-library/react";
import { format } from "date-fns";
import { beforeEach, describe, expect, it, vi } from "vitest";

import useOrderHistory from "@workspace/api-hooks/ordering/orders/useOrderHistory";
import type { OrderEventHistory } from "@workspace/types/ordering/orders";

import OrderTimeline from "@/features/ordering/order-detail/order-timeline";

import { renderWithProviders } from "../../utils/test-utils";

vi.mock("@workspace/api-hooks/ordering/orders/useOrderHistory", () => ({
  default: vi.fn(),
}));

const mockUseOrderHistory = vi.mocked(useOrderHistory);

describe("OrderTimeline", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should pass orderId and retry: false to useOrderHistory", () => {
    mockUseOrderHistory.mockReturnValue({
      data: undefined,
      isError: false,
    } as ReturnType<typeof useOrderHistory>);

    renderWithProviders(<OrderTimeline orderId="order-123" />);

    expect(mockUseOrderHistory).toHaveBeenCalledWith("order-123", {
      retry: false,
    });
  });

  it("should return null when isError is true", () => {
    mockUseOrderHistory.mockReturnValue({
      data: undefined,
      isError: true,
    } as ReturnType<typeof useOrderHistory>);

    const { container } = renderWithProviders(
      <OrderTimeline orderId="order-123" />,
    );

    expect(container).toBeEmptyDOMElement();
  });

  it("should return null when history is undefined", () => {
    mockUseOrderHistory.mockReturnValue({
      data: undefined,
      isError: false,
    } as ReturnType<typeof useOrderHistory>);

    const { container } = renderWithProviders(
      <OrderTimeline orderId="order-123" />,
    );

    expect(container).toBeEmptyDOMElement();
  });

  it("should return null when history is empty", () => {
    mockUseOrderHistory.mockReturnValue({
      data: [],
      isError: false,
    } as unknown as ReturnType<typeof useOrderHistory>);

    const { container } = renderWithProviders(
      <OrderTimeline orderId="order-123" />,
    );

    expect(container).toBeEmptyDOMElement();
  });

  it("should render Order Timeline heading and status labels for known statuses", () => {
    const timestamps = [
      "2024-01-15T10:30:00Z",
      "2024-01-16T14:45:00Z",
      "2024-01-17T09:00:00Z",
    ];
    const history: OrderEventHistory[] = [
      {
        sequence: 1,
        version: 1,
        eventType: "OrderPlacedEvent",
        timestamp: timestamps[0]!,
        status: "New",
      },
      {
        sequence: 2,
        version: 2,
        eventType: "OrderCompletedEvent",
        timestamp: timestamps[1]!,
        status: "Completed",
      },
      {
        sequence: 3,
        version: 3,
        eventType: "OrderCancelledEvent",
        timestamp: timestamps[2]!,
        status: "Cancelled",
      },
    ];

    mockUseOrderHistory.mockReturnValue({
      data: history,
      isError: false,
    } as unknown as ReturnType<typeof useOrderHistory>);

    renderWithProviders(<OrderTimeline orderId="order-123" />);

    expect(
      screen.getByRole("heading", { name: /order timeline/i }),
    ).toBeInTheDocument();
    expect(screen.getByText("Order Placed")).toBeInTheDocument();
    expect(screen.getByText("Order Completed")).toBeInTheDocument();
    expect(screen.getByText("Order Cancelled")).toBeInTheDocument();

    for (const ts of timestamps) {
      expect(
        screen.getByText(format(new Date(ts), "MMM dd, yyyy HH:mm")),
      ).toBeInTheDocument();
    }
  });

  it("should fall back to eventType when event status is null or undefined", () => {
    const timestamp = "2024-02-01T12:00:00Z";
    const history: OrderEventHistory[] = [
      {
        sequence: 1,
        version: 1,
        eventType: "PaymentProcessed",
        timestamp,
        status: null,
      },
      {
        sequence: 2,
        version: 2,
        eventType: "ShipmentDispatched",
        timestamp: "2024-02-02T15:30:00Z",
      },
    ];

    mockUseOrderHistory.mockReturnValue({
      data: history,
      isError: false,
    } as unknown as ReturnType<typeof useOrderHistory>);

    renderWithProviders(<OrderTimeline orderId="order-456" />);

    expect(screen.getByText("PaymentProcessed")).toBeInTheDocument();
    expect(screen.getByText("ShipmentDispatched")).toBeInTheDocument();
    expect(screen.getAllByRole("listitem")).toHaveLength(2);
  });
});
