import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";

import { createMockFeedback } from "@/__tests__/factories";
import { reviewsColumns } from "@/features/reviews/table/columns";

describe("Reviews Table Columns", () => {
  const mockReview = createMockFeedback({ rating: 4.5 });

  it("has correct column count", () => {
    expect(reviewsColumns).toHaveLength(4); // customer, rating, comment, actions
  });

  it("renders customer full name", () => {
    const customerColumn = reviewsColumns[0]!;
    const cell = customerColumn.cell as any;
    const fullName = `${mockReview.firstName} ${mockReview.lastName}`;
    render(
      cell({
        row: { original: mockReview, getValue: () => mockReview.firstName },
      } as any),
    );

    expect(screen.getByText(fullName)).toBeInTheDocument();
  });

  it("renders only first name when last name is missing", () => {
    const customerColumn = reviewsColumns[0]!;
    const cell = customerColumn.cell as any;
    const reviewWithoutLastName = { ...mockReview, lastName: null };
    render(
      cell({
        row: {
          original: reviewWithoutLastName,
          getValue: () => reviewWithoutLastName.firstName,
        },
      } as any),
    );

    expect(
      screen.getByText(reviewWithoutLastName.firstName),
    ).toBeInTheDocument();
  });

  it('renders "Anonymous" when first name is missing', () => {
    const customerColumn = reviewsColumns[0]!;
    const cell = customerColumn.cell as any;
    const reviewWithoutName = { ...mockReview, firstName: null };
    render(
      cell({
        row: { original: reviewWithoutName, getValue: () => null },
      } as any),
    );

    expect(screen.getByText("Anonymous")).toBeInTheDocument();
  });

  it("renders rating with star icon", () => {
    const ratingColumn = reviewsColumns[1]!;
    const cell = ratingColumn.cell as any;
    const { container } = render(
      cell({
        row: { original: mockReview, getValue: () => mockReview.rating },
      } as any),
    );

    expect(screen.getByText(mockReview.rating.toFixed(1))).toBeInTheDocument();
    expect(container.querySelector(".fill-yellow-400")).toBeInTheDocument();
  });

  it("formats rating to one decimal place", () => {
    const ratingColumn = reviewsColumns[1]!;
    const cell = ratingColumn.cell as any;
    const reviewWithWholeRating = { ...mockReview, rating: 5 };
    render(
      cell({
        row: {
          original: reviewWithWholeRating,
          getValue: () => reviewWithWholeRating.rating,
        },
      } as any),
    );

    expect(screen.getByText("5.0")).toBeInTheDocument();
  });

  it("renders truncated comment", () => {
    const commentColumn = reviewsColumns[2]!;
    const cell = commentColumn.cell as any;
    const { container } = render(
      cell({
        row: { original: mockReview, getValue: () => mockReview.comment },
      } as any),
    );

    expect(screen.getByText(mockReview.comment)).toBeInTheDocument();
    expect(container.querySelector(".truncate")).toBeInTheDocument();
  });

  it('renders "No comment" when comment is null', () => {
    const commentColumn = reviewsColumns[2]!;
    const cell = commentColumn.cell as any;
    const reviewWithoutComment = { ...mockReview, comment: null };
    render(
      cell({
        row: { original: reviewWithoutComment, getValue: () => null },
      } as any),
    );

    expect(screen.getByText("No comment")).toBeInTheDocument();
  });

  it("shows tooltip with full comment on hover", async () => {
    const user = userEvent.setup();
    const commentColumn = reviewsColumns[2]!;
    const cell = commentColumn.cell as any;
    render(
      cell({
        row: { original: mockReview, getValue: () => mockReview.comment },
      } as any),
    );

    const truncatedComment = screen.getByText(mockReview.comment);
    await user.hover(truncatedComment);

    // Tooltip appears with full comment
    expect(truncatedComment).toHaveClass("cursor-help");
  });

  it("has correct header labels", () => {
    expect(reviewsColumns[0]!.header).toBe("Customer");
    expect(reviewsColumns[1]!.header).toBe("Rating");
    expect(reviewsColumns[2]!.header).toBe("Comment");
  });

  it("has correct accessor keys", () => {
    expect((reviewsColumns[0] as any).accessorKey).toBe("firstName");
    expect((reviewsColumns[1] as any).accessorKey).toBe("rating");
    expect((reviewsColumns[2] as any).accessorKey).toBe("comment");
  });

  it("actions column has correct id", () => {
    expect(reviewsColumns[3]!.id).toBe("actions");
  });
});
