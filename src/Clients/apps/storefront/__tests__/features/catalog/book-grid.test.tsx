import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import BookGrid from "@/features/catalog/shop/book-grid";

import { renderWithProviders } from "../../utils/test-utils";

describe("BookGrid", () => {
  const mockOnPageChange = vi.fn();
  const mockOnClearFilters = vi.fn();

  const mockBooks = [
    {
      id: "1",
      name: "Book One",
      authors: [{ name: "Author One" }],
      price: 19.99,
      imageUrl: "/book1.jpg",
      averageRating: 4.5,
      category: { name: "Fiction" },
    },
    {
      id: "2",
      name: "Book Two",
      authors: [{ name: "Author Two" }],
      price: 29.99,
      imageUrl: "/book2.jpg",
      averageRating: 3.8,
      category: { name: "Non-Fiction" },
    },
  ];

  const defaultProps = {
    books: mockBooks,
    isLoading: false,
    totalPages: 1,
    currentPage: 1,
    onPageChange: mockOnPageChange,
    onClearFilters: mockOnClearFilters,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display loading state", () => {
    renderWithProviders(<BookGrid {...defaultProps} isLoading={true} />);

    const loadingContainer = screen.getByRole("status");
    expect(loadingContainer).toBeInTheDocument();
  });

  it("should display books grid", () => {
    renderWithProviders(<BookGrid {...defaultProps} />);

    expect(screen.getByRole("list")).toBeInTheDocument();
    expect(screen.getByText("Book One")).toBeInTheDocument();
  });

  it("should display empty state", () => {
    renderWithProviders(<BookGrid {...defaultProps} books={[]} />);

    expect(screen.getByText("No Books Found")).toBeInTheDocument();
  });

  it("should show clear filters button when empty", () => {
    renderWithProviders(<BookGrid {...defaultProps} books={[]} />);

    expect(
      screen.getByRole("button", { name: /clear filters/i }),
    ).toBeInTheDocument();
  });

  it("should call onClearFilters when button clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<BookGrid {...defaultProps} books={[]} />);

    const clearButton = screen.getByRole("button", { name: /clear filters/i });
    await user.click(clearButton);

    expect(mockOnClearFilters).toHaveBeenCalled();
  });

  it("should show pagination when multiple pages", () => {
    renderWithProviders(<BookGrid {...defaultProps} totalPages={5} />);

    expect(screen.getByRole("navigation")).toBeInTheDocument();
  });

  it("should not show pagination for single page", () => {
    renderWithProviders(<BookGrid {...defaultProps} totalPages={1} />);

    expect(screen.queryByRole("navigation")).not.toBeInTheDocument();
  });
});
