import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import ReviewsContainer from "@/features/catalog/product/reviews-container";

import { renderWithProviders } from "../../utils/test-utils";

describe("ReviewsContainer", () => {
  const mockReviews = [
    {
      id: faker.string.uuid(),
      firstName: faker.person.firstName(),
      lastName: faker.person.lastName(),
      rating: faker.number.int({ min: 1, max: 5 }),
      comment: faker.lorem.sentence(),
      bookId: faker.string.uuid(),
    },
    {
      id: faker.string.uuid(),
      firstName: faker.person.firstName(),
      lastName: faker.person.lastName(),
      rating: faker.number.int({ min: 1, max: 5 }),
      comment: faker.lorem.sentence(),
      bookId: faker.string.uuid(),
    },
  ];

  const mockReviewForm = {
    firstName: "",
    lastName: "",
    comment: "",
    rating: 0,
    isSubmitting: false,
    onChange: vi.fn(),
    onSubmit: vi.fn(),
  };

  const defaultProps = {
    isLoading: false,
    reviews: mockReviews,
    averageRating: 4.5,
    totalReviews: 2,
    sortBy: "newest" as const,
    onSortChange: vi.fn(),
    isSummarizing: false,
    onSummarize: vi.fn(),
    showReviewForm: false,
    onToggleReviewForm: vi.fn(),
    reviewForm: mockReviewForm,
    currentPage: 1,
    totalPages: 1,
    onPageChange: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display loading skeleton when loading", () => {
    const { container } = renderWithProviders(
      <ReviewsContainer {...defaultProps} isLoading={true} />,
    );

    expect(container.querySelector(".animate-pulse")).toBeInTheDocument();
  });

  it("should display empty state when no reviews", () => {
    renderWithProviders(<ReviewsContainer {...defaultProps} reviews={[]} />);

    expect(screen.getByText(/no reviews yet/i)).toBeInTheDocument();
  });

  it("should call onToggleReviewForm when write review clicked in empty state", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReviewsContainer {...defaultProps} reviews={[]} />);

    const writeReviewButton = screen.getByRole("button", {
      name: /write the first review for this book/i,
    });
    await user.click(writeReviewButton);

    expect(defaultProps.onToggleReviewForm).toHaveBeenCalled();
  });

  it("should display reviews when available", () => {
    const reviews = [
      { ...mockReviews[0]!, comment: "Great book!" },
      { ...mockReviews[1]!, comment: "Good read." },
    ];
    renderWithProviders(
      <ReviewsContainer {...defaultProps} reviews={reviews} />,
    );

    expect(screen.getByText("Great book!")).toBeInTheDocument();
    expect(screen.getByText("Good read.")).toBeInTheDocument();
  });

  it("should display review summary card", () => {
    renderWithProviders(<ReviewsContainer {...defaultProps} />);

    expect(screen.getByText("4.5")).toBeInTheDocument();
  });

  it("should display review form when showReviewForm is true", () => {
    renderWithProviders(
      <ReviewsContainer {...defaultProps} showReviewForm={true} />,
    );

    expect(
      screen.getByRole("textbox", { name: /comment/i }),
    ).toBeInTheDocument();
  });

  it("should not display review form when showReviewForm is false", () => {
    renderWithProviders(
      <ReviewsContainer {...defaultProps} showReviewForm={false} />,
    );

    expect(
      screen.queryByRole("textbox", { name: /comment/i }),
    ).not.toBeInTheDocument();
  });
});
