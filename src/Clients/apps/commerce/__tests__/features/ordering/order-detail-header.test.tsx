import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import OrderDetailHeader from "@/features/ordering/order-detail/order-detail-header";

import { renderWithProviders } from "../../utils/test-utils";

describe("OrderDetailHeader", () => {
  it("should display order ID", () => {
    renderWithProviders(
      <OrderDetailHeader
        orderId="123456"
        status="Completed"
        date="2024-01-15"
      />,
    );

    expect(screen.getByText("Order 123456")).toBeInTheDocument();
  });

  it("should display order status badge", () => {
    renderWithProviders(
      <OrderDetailHeader
        orderId="123456"
        status="Completed"
        date="2024-01-15"
      />,
    );

    expect(screen.getByText("Completed")).toBeInTheDocument();
  });

  it("should display formatted date", () => {
    renderWithProviders(
      <OrderDetailHeader orderId="123456" status="New" date="2024-01-15" />,
    );

    expect(screen.getByText(/placed on/i)).toBeInTheDocument();
    expect(screen.getByText(/january 15, 2024/i)).toBeInTheDocument();
  });

  it("should render back to orders link", () => {
    renderWithProviders(
      <OrderDetailHeader
        orderId="123456"
        status="Cancelled"
        date="2024-01-15"
      />,
    );

    const link = screen.getByRole("link");
    expect(link).toHaveAttribute("href", "/account/orders");
    expect(screen.getByText("Back to Orders")).toBeInTheDocument();
  });

  it("should display ArrowLeft icon", () => {
    const { container } = renderWithProviders(
      <OrderDetailHeader
        orderId="123456"
        status="Completed"
        date="2024-01-15"
      />,
    );

    const icons = container.querySelectorAll("svg");
    expect(icons.length).toBeGreaterThan(0);
  });

  it("should have serif font for order ID", () => {
    renderWithProviders(
      <OrderDetailHeader
        orderId="123456"
        status="Completed"
        date="2024-01-15"
      />,
    );

    const orderId = screen.getByText("Order 123456");
    expect(orderId).toHaveClass("font-serif");
  });

  it("should render with different statuses", () => {
    const { rerender } = renderWithProviders(
      <OrderDetailHeader orderId="123" status="New" date="2024-01-01" />,
    );
    expect(screen.getByText("New")).toBeInTheDocument();

    rerender(
      <OrderDetailHeader orderId="123" status="Cancelled" date="2024-01-01" />,
    );
    expect(screen.getByText("Cancelled")).toBeInTheDocument();
  });

  it("should have muted foreground for date", () => {
    renderWithProviders(
      <OrderDetailHeader
        orderId="123456"
        status="Completed"
        date="2024-01-15"
      />,
    );

    const dateText = screen.getByText(/placed on/i);
    expect(dateText).toHaveClass("text-muted-foreground");
  });

  it("should handle different date formats", () => {
    renderWithProviders(
      <OrderDetailHeader
        orderId="999"
        status="Completed"
        date="2023-12-25T10:30:00Z"
      />,
    );

    expect(screen.getByText(/december 25, 2023/i)).toBeInTheDocument();
  });
});
