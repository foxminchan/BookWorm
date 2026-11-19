import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
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

  it.each([
    ["Price Range", "Price Range", undefined],
    ["Category", "Category", undefined],
    ["Publisher", "Publisher", undefined],
    ["Author", "Author", undefined],
  ])("should display %s filter", (_label, text, _value) => {
    renderWithProviders(<ShopFilters {...defaultProps} />);

    expect(screen.getAllByText(text)[0]).toBeInTheDocument();
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

  it("should handle null item names, empty priceRange, and slider changes", async () => {
    const user = userEvent.setup();
    const { rerender } = renderWithProviders(
      <ShopFilters
        {...defaultProps}
        priceRange={[]}
        categories={[{ id: "cat-null", name: null }]}
      />,
    );

    expect(screen.getAllByText("$0.00")[0]).toBeInTheDocument();
    expect(screen.getAllByText("$100.00+")[0]).toBeInTheDocument();

    rerender(<ShopFilters {...defaultProps} priceRange={[10, 80]} />);
    const sliders = screen.getAllByRole("slider");
    sliders[0]!.focus();
    await user.keyboard("{ArrowRight}");
    expect(mockSetPriceRange).toHaveBeenCalled();
  });
});
