import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import ReviewsSection from "@/features/catalog/product/reviews-section";

import { renderWithProviders } from "../../utils/test-utils";

describe("ReviewsSection", () => {
  const mockOnSortChange = vi.fn();
  const mockOnSummarize = vi.fn();
  const mockOnToggleReviewForm = vi.fn();

  const defaultProps = {
    sortBy: "newest" as const,
    onSortChange: mockOnSortChange,
    isSummarizing: false,
    onSummarize: mockOnSummarize,
    showReviewForm: false,
    onToggleReviewForm: mockOnToggleReviewForm,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display customer feedback heading", () => {
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    expect(
      screen.getByRole("heading", { name: /customer feedback/i }),
    ).toBeInTheDocument();
  });

  it("should display description text", () => {
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    expect(
      screen.getByText(/what our readers are saying about this book/i),
    ).toBeInTheDocument();
  });

  it("should display sort button with current sort option", () => {
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    expect(
      screen.getByRole("button", { name: /sort reviews by/i }),
    ).toBeInTheDocument();
    expect(screen.getByText(/sort by: newest/i)).toBeInTheDocument();
  });

  it("should display highest sort option", () => {
    renderWithProviders(<ReviewsSection {...defaultProps} sortBy="highest" />);

    expect(screen.getByText(/sort by: highest/i)).toBeInTheDocument();
  });

  it("should display lowest sort option", () => {
    renderWithProviders(<ReviewsSection {...defaultProps} sortBy="lowest" />);

    expect(screen.getByText(/sort by: lowest/i)).toBeInTheDocument();
  });

  it("should open sort dropdown when clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    const sortButton = screen.getByRole("button", { name: /sort reviews by/i });
    await user.click(sortButton);

    expect(await screen.findByText("Newest")).toBeInTheDocument();
    expect(screen.getByText("Highest Rating")).toBeInTheDocument();
    expect(screen.getByText("Lowest Rating")).toBeInTheDocument();
  });

  it("should call onSortChange when newest is selected", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReviewsSection {...defaultProps} sortBy="highest" />);

    await user.click(screen.getByRole("button", { name: /sort reviews by/i }));
    await user.click(await screen.findByText("Newest"));

    expect(mockOnSortChange).toHaveBeenCalledWith("newest");
  });

  it("should call onSortChange when highest is selected", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /sort reviews by/i }));
    await user.click(await screen.findByText("Highest Rating"));

    expect(mockOnSortChange).toHaveBeenCalledWith("highest");
  });

  it("should call onSortChange when lowest is selected", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /sort reviews by/i }));
    await user.click(await screen.findByText("Lowest Rating"));

    expect(mockOnSortChange).toHaveBeenCalledWith("lowest");
  });

  it("should display summarize reviews button", () => {
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    expect(
      screen.getByRole("button", { name: /summarize reviews/i }),
    ).toBeInTheDocument();
  });

  it("should call onSummarize when summarize button is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    await user.click(
      screen.getByRole("button", { name: /summarize reviews/i }),
    );

    expect(mockOnSummarize).toHaveBeenCalled();
  });

  it("should show loading state when summarizing", () => {
    renderWithProviders(
      <ReviewsSection {...defaultProps} isSummarizing={true} />,
    );

    expect(screen.getByText(/summarizing.../i)).toBeInTheDocument();
  });

  it("should disable summarize button when summarizing", () => {
    renderWithProviders(
      <ReviewsSection {...defaultProps} isSummarizing={true} />,
    );

    const button = screen.getByRole("button", { name: /summarize reviews/i });
    expect(button).toBeDisabled();
  });

  it("should display write review button", () => {
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    expect(
      screen.getByRole("button", { name: /write a review/i }),
    ).toBeInTheDocument();
  });

  it("should call onToggleReviewForm when write review is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /write a review/i }));

    expect(mockOnToggleReviewForm).toHaveBeenCalled();
  });

  it("should show cancel review text when form is open", () => {
    renderWithProviders(
      <ReviewsSection {...defaultProps} showReviewForm={true} />,
    );

    expect(screen.getByText(/cancel review/i)).toBeInTheDocument();
  });

  it("should display AI summary when provided", () => {
    renderWithProviders(
      <ReviewsSection
        {...defaultProps}
        summary="This is a great book with excellent reviews."
      />,
    );

    expect(
      screen.getByText(/this is a great book with excellent reviews/i),
    ).toBeInTheDocument();
  });

  it("should display AI review summary heading when summary exists", () => {
    renderWithProviders(
      <ReviewsSection {...defaultProps} summary="Great book!" />,
    );

    expect(
      screen.getByRole("heading", { name: /ai review summary/i }),
    ).toBeInTheDocument();
  });

  it("should not display summary when not provided", () => {
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    expect(
      screen.queryByRole("heading", { name: /ai review summary/i }),
    ).not.toBeInTheDocument();
  });

  it("should have sparkles icon on summarize button", () => {
    const { container } = renderWithProviders(
      <ReviewsSection {...defaultProps} />,
    );

    const icons = container.querySelectorAll("svg");
    expect(icons.length).toBeGreaterThan(0);
  });

  it("should have loader icon when summarizing", () => {
    const { container } = renderWithProviders(
      <ReviewsSection {...defaultProps} isSummarizing={true} />,
    );

    const loader = container.querySelector(".animate-spin");
    expect(loader).toBeInTheDocument();
  });

  it("should have message square icon on write review button", () => {
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    const button = screen.getByRole("button", { name: /write a review/i });
    expect(button).toBeInTheDocument();
  });

  it("should have rounded buttons", () => {
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    const sortButton = screen.getByRole("button", { name: /sort reviews by/i });
    expect(sortButton).toHaveClass("rounded-full");
  });

  it("should render header with serif font", () => {
    const { container } = renderWithProviders(
      <ReviewsSection {...defaultProps} />,
    );

    const heading = container.querySelector(".font-serif");
    expect(heading).toBeInTheDocument();
  });

  it("should have proper aria labels", () => {
    renderWithProviders(<ReviewsSection {...defaultProps} />);

    expect(
      screen.getByRole("button", { name: /sort reviews by/i }),
    ).toHaveAttribute("aria-label", "Sort reviews by");
    expect(
      screen.getByRole("button", { name: /summarize reviews/i }),
    ).toHaveAttribute("aria-label", "Summarize reviews");
  });

  it("should have ghost variant when review form is shown", () => {
    renderWithProviders(
      <ReviewsSection {...defaultProps} showReviewForm={true} />,
    );

    const button = screen.getByRole("button", {
      name: /cancel writing a review/i,
    });
    expect(button).toBeInTheDocument();
  });

  it("should display summary with italic text", () => {
    const { container } = renderWithProviders(
      <ReviewsSection {...defaultProps} summary="Great book!" />,
    );

    const summary = container.querySelector(".italic");
    expect(summary).toBeInTheDocument();
    expect(summary?.textContent).toContain("Great book!");
  });

  it("should have animation on summary", () => {
    const { container } = renderWithProviders(
      <ReviewsSection {...defaultProps} summary="Great book!" />,
    );

    const summaryContainer = container.querySelector(".animate-in");
    expect(summaryContainer).toBeInTheDocument();
  });
});
