import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import ShopToolbar from "@/features/catalog/shop/shop-toolbar";

import { renderWithProviders } from "../../utils/test-utils";

describe("ShopToolbar", () => {
  const mockOnClearSearch = vi.fn();
  const mockOnSortChange = vi.fn();
  const mockOnOpenFilters = vi.fn();

  const defaultProps = {
    searchQuery: "",
    onClearSearch: mockOnClearSearch,
    totalCount: 100,
    currentPage: 1,
    sortBy: "name",
    onSortChange: mockOnSortChange,
    onOpenFilters: mockOnOpenFilters,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display shop title", () => {
    renderWithProviders(<ShopToolbar {...defaultProps} />);

    expect(screen.getByText("Shop All Books")).toBeInTheDocument();
  });

  it("should display results count", () => {
    renderWithProviders(<ShopToolbar {...defaultProps} />);

    expect(screen.getByText(/showing 1–8 of 100 results/i)).toBeInTheDocument();
  });

  it("should display search results when searching", () => {
    renderWithProviders(
      <ShopToolbar {...defaultProps} searchQuery="react" totalCount={5} />,
    );

    expect(screen.getByText("Search results for")).toBeInTheDocument();
    expect(screen.getByText('"react"')).toBeInTheDocument();
  });

  it("should call onClearSearch when clear button clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ShopToolbar {...defaultProps} searchQuery="test" />);

    const clearButton = screen.getByRole("button", { name: /clear search/i });
    await user.click(clearButton);

    expect(mockOnClearSearch).toHaveBeenCalled();
  });

  it("should display filters button", () => {
    renderWithProviders(<ShopToolbar {...defaultProps} />);

    expect(
      screen.getByRole("button", { name: /open filters/i }),
    ).toBeInTheDocument();
  });

  it("should call onOpenFilters when filters clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ShopToolbar {...defaultProps} />);

    const filtersButton = screen.getByRole("button", { name: /open filters/i });
    await user.click(filtersButton);

    expect(mockOnOpenFilters).toHaveBeenCalled();
  });

  it("should display sort select", () => {
    renderWithProviders(<ShopToolbar {...defaultProps} />);

    expect(screen.getByRole("combobox")).toBeInTheDocument();
  });
});
