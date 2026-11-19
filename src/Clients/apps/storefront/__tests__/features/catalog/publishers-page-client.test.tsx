import { beforeEach, describe, expect, it, vi } from "vitest";

import { renderWithProviders, screen } from "@/__tests__/utils/test-utils";
import PublishersPageClient from "@/features/catalog/publisher/publishers-page-client";

const mockUsePublishers = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/catalog/publishers/usePublishers", () => ({
  default: mockUsePublishers,
}));

vi.mock("@/features/catalog/publisher/publishers-header", () => ({
  default: () => <div data-testid="publishers-header" />,
}));

vi.mock("@/features/catalog/publisher/publishers-grid", () => ({
  default: ({
    publishers,
    isLoading,
  }: {
    publishers: unknown[];
    isLoading: boolean;
  }) => (
    <div
      data-testid="publishers-grid"
      data-count={publishers.length}
      data-loading={String(isLoading)}
    />
  ),
}));

vi.mock("@/features/catalog/publisher/publishers-info-section", () => ({
  default: () => <div data-testid="publishers-info-section" />,
}));

describe("PublishersPageClient", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders loading state with header, grid, and info section", () => {
    mockUsePublishers.mockReturnValue({
      data: undefined,
      isLoading: true,
    });

    renderWithProviders(<PublishersPageClient />);

    expect(screen.getByTestId("publishers-header")).toBeInTheDocument();
    const grid = screen.getByTestId("publishers-grid");
    expect(grid).toHaveAttribute("data-count", "0");
    expect(grid).toHaveAttribute("data-loading", "true");
    expect(screen.getByTestId("publishers-info-section")).toBeInTheDocument();
    expect(screen.queryByText("Coming Soon")).not.toBeInTheDocument();
  });

  it("renders empty state when loaded with no publishers", () => {
    mockUsePublishers.mockReturnValue({
      data: [],
      isLoading: false,
    });

    renderWithProviders(<PublishersPageClient />);

    expect(screen.getByText("Coming Soon")).toBeInTheDocument();
    expect(
      screen.getByText(
        "We're still discovering amazing publishers to bring to BookWorm. Check back soon for an exciting collection.",
      ),
    ).toBeInTheDocument();
    const homeLink = screen.getByRole("link", { name: "Return to Home" });
    expect(homeLink).toHaveAttribute("href", "/");
    expect(screen.queryByTestId("publishers-header")).not.toBeInTheDocument();
    expect(screen.queryByTestId("publishers-grid")).not.toBeInTheDocument();
    expect(
      screen.queryByTestId("publishers-info-section"),
    ).not.toBeInTheDocument();
  });

  it("renders empty state when publishers data is undefined and not loading", () => {
    mockUsePublishers.mockReturnValue({
      data: undefined,
      isLoading: false,
    });

    renderWithProviders(<PublishersPageClient />);

    expect(screen.getByText("Coming Soon")).toBeInTheDocument();
    expect(screen.queryByTestId("publishers-grid")).not.toBeInTheDocument();
  });

  it("renders publishers grid when publishers are loaded", () => {
    mockUsePublishers.mockReturnValue({
      data: [
        { id: "pub-1", name: "O'Reilly Media" },
        { id: "pub-2", name: "Manning" },
      ],
      isLoading: false,
    });

    renderWithProviders(<PublishersPageClient />);

    expect(screen.getByTestId("publishers-header")).toBeInTheDocument();
    const grid = screen.getByTestId("publishers-grid");
    expect(grid).toHaveAttribute("data-count", "2");
    expect(grid).toHaveAttribute("data-loading", "false");
    expect(screen.getByTestId("publishers-info-section")).toBeInTheDocument();
    expect(screen.queryByText("Coming Soon")).not.toBeInTheDocument();
  });
});
