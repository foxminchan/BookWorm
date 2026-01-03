import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import ReviewsEmptyState from "@/features/catalog/product/reviews-empty-state";

import { renderWithProviders } from "../../utils/test-utils";

describe("ReviewsEmptyState", () => {
  const mockOnWriteReview = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render empty state message", () => {
    renderWithProviders(
      <ReviewsEmptyState onWriteReview={mockOnWriteReview} />,
    );

    expect(screen.getByText("No reviews yet")).toBeInTheDocument();
  });

  it("should display encouragement text", () => {
    renderWithProviders(
      <ReviewsEmptyState onWriteReview={mockOnWriteReview} />,
    );

    expect(
      screen.getByText(/be the first to share your thoughts/i),
    ).toBeInTheDocument();
  });

  it("should render write review button", () => {
    renderWithProviders(
      <ReviewsEmptyState onWriteReview={mockOnWriteReview} />,
    );

    expect(
      screen.getByRole("button", { name: /write first review/i }),
    ).toBeInTheDocument();
  });

  it("should call onWriteReview when button is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(
      <ReviewsEmptyState onWriteReview={mockOnWriteReview} />,
    );

    const button = screen.getByRole("button", { name: /write first review/i });
    await user.click(button);

    expect(mockOnWriteReview).toHaveBeenCalledTimes(1);
  });

  it("should display message square icon", () => {
    const { container } = renderWithProviders(
      <ReviewsEmptyState onWriteReview={mockOnWriteReview} />,
    );

    const icons = container.querySelectorAll("svg");
    expect(icons.length).toBeGreaterThan(0);
  });

  it("should have centered text alignment", () => {
    const { container } = renderWithProviders(
      <ReviewsEmptyState onWriteReview={mockOnWriteReview} />,
    );

    const textContainer = container.querySelector(".text-center");
    expect(textContainer).toBeInTheDocument();
  });

  it("should have proper spacing", () => {
    const { container } = renderWithProviders(
      <ReviewsEmptyState onWriteReview={mockOnWriteReview} />,
    );

    const container_div = container.querySelector(".space-y-4");
    expect(container_div).toBeInTheDocument();
  });

  it("should have rounded button", () => {
    renderWithProviders(
      <ReviewsEmptyState onWriteReview={mockOnWriteReview} />,
    );

    const button = screen.getByRole("button", { name: /write first review/i });
    expect(button).toHaveClass("rounded-full");
  });

  it("should display button with icon", () => {
    const { container } = renderWithProviders(
      <ReviewsEmptyState onWriteReview={mockOnWriteReview} />,
    );

    const button = screen.getByRole("button", { name: /write first review/i });
    const icon = button.querySelector("svg");
    expect(icon).toBeInTheDocument();
  });

  it("should render with proper vertical padding", () => {
    const { container } = renderWithProviders(
      <ReviewsEmptyState onWriteReview={mockOnWriteReview} />,
    );

    const container_div = container.querySelector(".py-16");
    expect(container_div).toBeInTheDocument();
  });

  it("should have large icon size", () => {
    const { container } = renderWithProviders(
      <ReviewsEmptyState onWriteReview={mockOnWriteReview} />,
    );

    const largeIcon = container.querySelector(".size-12");
    expect(largeIcon).toBeInTheDocument();
  });
});
