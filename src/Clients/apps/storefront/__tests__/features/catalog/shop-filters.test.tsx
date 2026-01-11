import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import ShopFilters from "@/features/catalog/shop/shop-filters";

import { renderWithProviders } from "../../utils/test-utils";

describe("ShopFilters", () => {
  const mockSetPriceRange = vi.fn();
  const mockOnToggleCategory = vi.fn();
  const mockOnTogglePublisher = vi.fn();
  const mockOnToggleAuthor = vi.fn();
  const mockSetIsFilterOpen = vi.fn();

  const mockCategories = [
    { id: faker.string.uuid(), name: faker.commerce.department() },
    { id: faker.string.uuid(), name: faker.commerce.department() },
  ];

  const mockPublishers = [
    { id: faker.string.uuid(), name: faker.company.name() },
    { id: faker.string.uuid(), name: faker.company.name() },
  ];

  const mockAuthors = [
    { id: faker.string.uuid(), name: faker.person.fullName() },
    { id: faker.string.uuid(), name: faker.person.fullName() },
  ];

  const defaultProps = {
    priceRange: [0, 100],
    setPriceRange: mockSetPriceRange,
    categories: mockCategories,
    selectedCategories: [],
    onToggleCategory: mockOnToggleCategory,
    publishers: mockPublishers,
    selectedPublishers: [],
    onTogglePublisher: mockOnTogglePublisher,
    authors: mockAuthors,
    selectedAuthors: [],
    onToggleAuthor: mockOnToggleAuthor,
    isFilterOpen: false,
    setIsFilterOpen: mockSetIsFilterOpen,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display price range filter", () => {
    renderWithProviders(<ShopFilters {...defaultProps} />);

    expect(screen.getAllByText("Price Range")[0]).toBeInTheDocument();
    expect(screen.getAllByText("$0.00")[0]).toBeInTheDocument();
  });

  it("should display category filter", () => {
    renderWithProviders(<ShopFilters {...defaultProps} />);

    expect(screen.getAllByText("Category")[0]).toBeInTheDocument();
  });

  it("should display publisher filter", () => {
    renderWithProviders(<ShopFilters {...defaultProps} />);

    expect(screen.getAllByText("Publisher")[0]).toBeInTheDocument();
  });

  it("should display author filter", () => {
    renderWithProviders(<ShopFilters {...defaultProps} />);

    expect(screen.getAllByText("Author")[0]).toBeInTheDocument();
  });

  it("should show selected price range", () => {
    renderWithProviders(
      <ShopFilters {...defaultProps} priceRange={[20, 80]} />,
    );

    expect(screen.getAllByText("$20.00")[0]).toBeInTheDocument();
  });

  it("should handle empty categories", () => {
    renderWithProviders(
      <ShopFilters {...defaultProps} categories={undefined} />,
    );

    expect(screen.getAllByText("Category")[0]).toBeInTheDocument();
  });

  it("should render desktop sidebar", () => {
    const { container } = renderWithProviders(
      <ShopFilters {...defaultProps} />,
    );

    const aside = container.querySelector("aside");
    expect(aside).toBeInTheDocument();
  });
});
