import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import ConfirmationHeader from "@/features/ordering/checkout/confirmation-header";

import { renderWithProviders } from "../../utils/test-utils";

describe("ConfirmationHeader", () => {
  it("should display order confirmed heading", () => {
    renderWithProviders(<ConfirmationHeader orderId="abc123def456" />);

    expect(
      screen.getByRole("heading", { name: /order confirmed/i }),
    ).toBeInTheDocument();
  });

  it("should display success message", () => {
    renderWithProviders(<ConfirmationHeader orderId="abc123def456" />);

    expect(
      screen.getByText(/your books are on their way to you/i),
    ).toBeInTheDocument();
  });

  it("should display order number label", () => {
    renderWithProviders(<ConfirmationHeader orderId="abc123def456" />);

    expect(screen.getByText(/order number/i)).toBeInTheDocument();
  });

  it("should display formatted order ID", () => {
    renderWithProviders(<ConfirmationHeader orderId="abc123def456" />);

    expect(screen.getByText(/#ABC123DE/i)).toBeInTheDocument();
  });

  it("should display truncated order ID to 8 characters", () => {
    renderWithProviders(
      <ConfirmationHeader orderId="verylongorderid123456789" />,
    );

    expect(screen.getByText(/#VERYLONG/i)).toBeInTheDocument();
  });

  it("should display check circle icon", () => {
    const { container } = renderWithProviders(
      <ConfirmationHeader orderId="abc123def456" />,
    );

    const icon = container.querySelector("svg");
    expect(icon).toBeInTheDocument();
  });
});
