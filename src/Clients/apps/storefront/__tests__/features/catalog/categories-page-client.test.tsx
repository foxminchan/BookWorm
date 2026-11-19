import { beforeEach, describe, expect, it, vi } from "vitest";

import { renderWithProviders, screen } from "@/__tests__/utils/test-utils";
import CategoriesPageClient from "@/features/catalog/categories/categories-page-client";

const mockUseCategories = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/catalog/categories/useCategories", () => ({
  default: mockUseCategories,
}));

vi.mock("@/features/catalog/categories/categories-header", () => ({
  default: () => <div data-testid="categories-header" />,
}));

vi.mock("@/features/catalog/categories/categories-grid", () => ({
  default: ({
    categories,
    isLoading,
  }: {
    categories: unknown[];
    isLoading: boolean;
  }) => (
    <div
      data-testid="categories-grid"
      data-count={categories.length}
      data-loading={String(isLoading)}
    />
  ),
}));

describe("CategoriesPageClient", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders loading state with categories grid", () => {
    mockUseCategories.mockReturnValue({
      data: undefined,
      isLoading: true,
    });

    renderWithProviders(<CategoriesPageClient />);

    expect(screen.getByTestId("categories-header")).toBeInTheDocument();
    const grid = screen.getByTestId("categories-grid");
    expect(grid).toHaveAttribute("data-count", "0");
    expect(grid).toHaveAttribute("data-loading", "true");
    expect(screen.queryByText("Coming Soon")).not.toBeInTheDocument();
  });

  it("renders empty state when loaded with no categories", () => {
    mockUseCategories.mockReturnValue({
      data: [],
      isLoading: false,
    });

    renderWithProviders(<CategoriesPageClient />);

    expect(screen.getByTestId("categories-header")).toBeInTheDocument();
    expect(screen.getByText("Coming Soon")).toBeInTheDocument();
    expect(
      screen.getByText(
        "We're setting up our collection. In the meantime, explore our shop for all available books.",
      ),
    ).toBeInTheDocument();
    const shopLink = screen.getByRole("link", { name: "Browse the Shop" });
    expect(shopLink).toHaveAttribute("href", "/shop");
    expect(screen.queryByTestId("categories-grid")).not.toBeInTheDocument();
  });

  it("renders empty state when categories data is undefined and not loading", () => {
    mockUseCategories.mockReturnValue({
      data: undefined,
      isLoading: false,
    });

    renderWithProviders(<CategoriesPageClient />);

    expect(screen.getByText("Coming Soon")).toBeInTheDocument();
    expect(screen.queryByTestId("categories-grid")).not.toBeInTheDocument();
  });

  it("renders categories grid when categories are loaded", () => {
    mockUseCategories.mockReturnValue({
      data: [
        { id: "cat-1", name: "Fiction" },
        { id: "cat-2", name: "Science" },
      ],
      isLoading: false,
    });

    renderWithProviders(<CategoriesPageClient />);

    expect(screen.getByTestId("categories-header")).toBeInTheDocument();
    const grid = screen.getByTestId("categories-grid");
    expect(grid).toHaveAttribute("data-count", "2");
    expect(grid).toHaveAttribute("data-loading", "false");
    expect(screen.queryByText("Coming Soon")).not.toBeInTheDocument();
  });
});
