import { faker } from "@faker-js/faker";
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
      id: faker.string.uuid(),
      name: faker.commerce.productName(),
      description: faker.commerce.productDescription(),
      authors: [{ id: faker.string.uuid(), name: faker.person.fullName() }],
      price: faker.number.float({ min: 10, max: 50, fractionDigits: 2 }),
      priceSale: null,
      status: "InStock" as const,
      imageUrl: faker.image.url(),
      averageRating: faker.number.float({ min: 1, max: 5, fractionDigits: 1 }),
      totalReviews: faker.number.int({ min: 0, max: 100 }),
      category: { id: faker.string.uuid(), name: faker.commerce.department() },
      publisher: { id: faker.string.uuid(), name: faker.company.name() },
    },
    {
      id: faker.string.uuid(),
      name: faker.commerce.productName(),
      description: faker.commerce.productDescription(),
      authors: [{ id: faker.string.uuid(), name: faker.person.fullName() }],
      price: faker.number.float({ min: 10, max: 50, fractionDigits: 2 }),
      priceSale: null,
      status: "InStock" as const,
      imageUrl: faker.image.url(),
      averageRating: faker.number.float({ min: 1, max: 5, fractionDigits: 1 }),
      totalReviews: faker.number.int({ min: 0, max: 100 }),
      category: { id: faker.string.uuid(), name: faker.commerce.department() },
      publisher: { id: faker.string.uuid(), name: faker.company.name() },
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

    const loadingContainers = screen.getAllByRole("status");
    expect(loadingContainers.length).toBeGreaterThan(0);
  });

  it("should display books grid", () => {
    const books = [{ ...mockBooks[0]!, name: "Book One" }, mockBooks[1]!];
    renderWithProviders(<BookGrid {...defaultProps} books={books} />);

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
      screen.getByRole("button", { name: /clear all filters/i }),
    ).toBeInTheDocument();
  });

  it("should call onClearFilters when button clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<BookGrid {...defaultProps} books={[]} />);

    const clearButton = screen.getByRole("button", {
      name: /clear all filters/i,
    });
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
