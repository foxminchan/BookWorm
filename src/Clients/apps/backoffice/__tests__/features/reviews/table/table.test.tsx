import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createMockFeedback } from "@/__tests__/factories";
import { renderWithProviders } from "@/__tests__/utils/test-utils";
import { ReviewsTable } from "@/features/reviews/table/table";

// Mock the data hook
const mockUseFeedbacks = vi.hoisted(() => vi.fn());
vi.mock("@workspace/api-hooks/rating/useFeedbacks", () => ({
  default: mockUseFeedbacks,
}));

// Mock the delete hook used by CellAction
vi.mock("@workspace/api-hooks/rating/useDeleteFeedback", () => ({
  default: () => ({ mutate: vi.fn(), isPending: false }),
}));

describe("ReviewsTable", () => {
  const bookId = "test-book-id";

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render loading state", () => {
    mockUseFeedbacks.mockReturnValue({
      data: undefined,
      isLoading: true,
    });

    renderWithProviders(<ReviewsTable bookId={bookId} />);

    expect(screen.getByRole("status")).toBeInTheDocument();
  });

  it("should render feedback data in table", () => {
    const feedbacks = [
      createMockFeedback({ firstName: "Alice", lastName: "Johnson" }),
      createMockFeedback({ firstName: "Bob", lastName: "Smith" }),
    ];

    mockUseFeedbacks.mockReturnValue({
      data: { items: feedbacks, totalCount: 2 },
      isLoading: false,
    });

    renderWithProviders(<ReviewsTable bookId={bookId} />);

    expect(screen.getByText("Alice Johnson")).toBeInTheDocument();
    expect(screen.getByText("Bob Smith")).toBeInTheDocument();
  });

  it("should display total count in description", () => {
    mockUseFeedbacks.mockReturnValue({
      data: { items: [], totalCount: 23 },
      isLoading: false,
    });

    renderWithProviders(<ReviewsTable bookId={bookId} />);

    expect(
      screen.getAllByText("Total: 23 reviews").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("should render table title", () => {
    mockUseFeedbacks.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    });

    renderWithProviders(<ReviewsTable bookId={bookId} />);

    expect(screen.getByText("Book Reviews")).toBeInTheDocument();
  });

  it("should handle empty data gracefully", () => {
    mockUseFeedbacks.mockReturnValue({
      data: undefined,
      isLoading: false,
    });

    renderWithProviders(<ReviewsTable bookId={bookId} />);

    expect(
      screen.getAllByText("Total: 0 reviews").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("should pass bookId and pagination params to the hook", () => {
    mockUseFeedbacks.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    });

    renderWithProviders(<ReviewsTable bookId={bookId} />);

    expect(mockUseFeedbacks).toHaveBeenCalledWith(
      expect.objectContaining({
        bookId,
        pageIndex: 1,
        pageSize: 10,
      }),
    );
  });
});
