import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import ReviewList from "@/features/catalog/product/review-list";

import { renderWithProviders } from "../../utils/test-utils";

const mockReviews = [
  {
    id: faker.string.uuid(),
    firstName: faker.person.firstName(),
    lastName: faker.person.lastName(),
    rating: faker.number.int({ min: 1, max: 5 }),
    comment: faker.lorem.sentence(),
  },
  {
    id: faker.string.uuid(),
    firstName: faker.person.firstName(),
    lastName: faker.person.lastName(),
    rating: faker.number.int({ min: 1, max: 5 }),
    comment: faker.lorem.sentence(),
  },
  {
    id: faker.string.uuid(),
    firstName: faker.person.firstName(),
    lastName: faker.person.lastName(),
    rating: faker.number.int({ min: 1, max: 5 }),
    comment: faker.lorem.sentence(),
  },
];

describe("ReviewList", () => {
  const mockOnPageChange = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render all reviews", () => {
    const reviews = [
      { ...mockReviews[0]!, firstName: "John", lastName: "Doe" },
      { ...mockReviews[1]!, firstName: "Jane", lastName: "Smith" },
      { ...mockReviews[2]!, firstName: "Bob", lastName: "Johnson" },
    ];

    renderWithProviders(
      <ReviewList
        reviews={reviews}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    expect(screen.getByText("John Doe")).toBeInTheDocument();
    expect(screen.getByText("Jane Smith")).toBeInTheDocument();
    expect(screen.getByText("Bob Johnson")).toBeInTheDocument();
  });

  it("should display review comments", () => {
    const reviews = [
      { ...mockReviews[0]!, comment: "Excellent book! Highly recommended." },
      { ...mockReviews[1]!, comment: "Great read, very informative." },
      { ...mockReviews[2]!, comment: "Good book but could be better." },
    ];

    renderWithProviders(
      <ReviewList
        reviews={reviews}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    expect(
      screen.getByText("Excellent book! Highly recommended."),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Great read, very informative."),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Good book but could be better."),
    ).toBeInTheDocument();
  });

  it("should render stars for each review", () => {
    const { container } = renderWithProviders(
      <ReviewList
        reviews={mockReviews}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    const stars = container.querySelectorAll("svg");
    // 3 reviews * 5 stars each = 15 stars, plus user icons
    expect(stars.length).toBeGreaterThan(15);
  });

  it("should display filled stars based on rating", () => {
    const reviews = [{ ...mockReviews[0]!, rating: 5 }];

    const { container } = renderWithProviders(
      <ReviewList
        reviews={reviews}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    // Review has 5 stars
    const filledStars = container.querySelectorAll(".fill-primary");
    expect(filledStars).toHaveLength(5);
  });

  it("should render user icons", () => {
    const { container } = renderWithProviders(
      <ReviewList
        reviews={mockReviews}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    const userIcons = container.querySelectorAll(".rounded-full");
    expect(userIcons.length).toBeGreaterThanOrEqual(3);
  });

  it("should not render pagination when totalPages is 1", () => {
    renderWithProviders(
      <ReviewList
        reviews={mockReviews}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    expect(screen.queryByRole("navigation")).not.toBeInTheDocument();
  });

  it("should render pagination when totalPages > 1", () => {
    renderWithProviders(
      <ReviewList
        reviews={mockReviews}
        currentPage={1}
        totalPages={3}
        onPageChange={mockOnPageChange}
      />,
    );

    expect(screen.getByRole("navigation")).toBeInTheDocument();
  });

  it("should call onPageChange when page button is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(
      <ReviewList
        reviews={mockReviews}
        currentPage={1}
        totalPages={3}
        onPageChange={mockOnPageChange}
      />,
    );

    const nextButton = screen.getByRole("button", { name: /next/i });
    await user.click(nextButton);

    expect(mockOnPageChange).toHaveBeenCalledWith(2);
  });

  it("should render empty list when no reviews", () => {
    const { container } = renderWithProviders(
      <ReviewList
        reviews={[]}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    expect(container.querySelector(".space-y-8")).toBeInTheDocument();
    expect(screen.queryByText("John Doe")).not.toBeInTheDocument();
  });

  it("should handle single review", () => {
    const reviews = [
      { ...mockReviews[0]!, firstName: "John", lastName: "Doe" },
    ];

    renderWithProviders(
      <ReviewList
        reviews={reviews}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    expect(screen.getByText("John Doe")).toBeInTheDocument();
    expect(screen.queryByText("Jane Smith")).not.toBeInTheDocument();
  });

  it("should render separators between reviews", () => {
    const { container } = renderWithProviders(
      <ReviewList
        reviews={mockReviews}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    // Separators are rendered except for the last review
    const separators = container.querySelectorAll(".group-last\\:hidden");
    expect(separators).toHaveLength(3); // Same as number of reviews
  });

  it("should display reviewer names in correct format", () => {
    const reviews = [
      { ...mockReviews[0]!, firstName: "John", lastName: "Doe" },
      { ...mockReviews[1]!, firstName: "Jane", lastName: "Smith" },
      mockReviews[2]!,
    ];

    renderWithProviders(
      <ReviewList
        reviews={reviews}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    expect(screen.getByText("John Doe")).toBeInTheDocument();
    expect(screen.getByText("Jane Smith")).toBeInTheDocument();
  });

  it("should have proper grid layout for larger screens", () => {
    const { container } = renderWithProviders(
      <ReviewList
        reviews={mockReviews}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    const reviewContainer = container.querySelector(".lg\\:col-span-2");
    expect(reviewContainer).toBeInTheDocument();
  });

  it("should style review comments correctly", () => {
    const reviews = [
      { ...mockReviews[0]!, comment: "Excellent book! Highly recommended." },
    ];

    renderWithProviders(
      <ReviewList
        reviews={reviews}
        currentPage={1}
        totalPages={1}
        onPageChange={mockOnPageChange}
      />,
    );

    const comment = screen.getByText("Excellent book! Highly recommended.");
    expect(comment).toHaveClass("text-muted-foreground");
  });
});
