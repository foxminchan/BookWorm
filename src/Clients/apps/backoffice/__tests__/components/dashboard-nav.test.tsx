import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { DashboardNav } from "@/components/dashboard-nav";

// Mock Next.js navigation
const mockPathname = vi.fn();
vi.mock("next/navigation", () => ({
  usePathname: () => mockPathname(),
}));

describe("DashboardNav", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render navigation with all menu items", () => {
    mockPathname.mockReturnValue("/");

    render(<DashboardNav />);

    // Overview
    expect(screen.getByRole("link", { name: /overview/i })).toBeInTheDocument();

    // Catalog section
    expect(screen.getByText("Catalog")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /books/i })).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: /categories/i }),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /authors/i })).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: /publishers/i }),
    ).toBeInTheDocument();

    // Ordering section
    expect(screen.getByText("Ordering")).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: /customers/i }),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /orders/i })).toBeInTheDocument();

    // Ratings & Reviews section
    expect(screen.getByText("Ratings & Reviews")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /reviews/i })).toBeInTheDocument();
  });

  it("should mark overview as active when on home page", () => {
    mockPathname.mockReturnValue("/");

    render(<DashboardNav />);

    const overviewLink = screen.getByRole("link", { name: /overview/i });
    expect(overviewLink).toHaveClass("bg-primary");
  });

  it("should mark overview as active when on (admin) route", () => {
    mockPathname.mockReturnValue("/(admin)");

    render(<DashboardNav />);

    const overviewLink = screen.getByRole("link", { name: /overview/i });
    expect(overviewLink).toHaveClass("bg-primary");
  });

  it("should mark books as active when on books page", () => {
    mockPathname.mockReturnValue("/books");

    render(<DashboardNav />);

    const booksLink = screen.getByRole("link", { name: /books/i });
    expect(booksLink).toHaveClass("bg-primary");
  });

  it("should mark categories as active when on categories page", () => {
    mockPathname.mockReturnValue("/categories");

    render(<DashboardNav />);

    const categoriesLink = screen.getByRole("link", { name: /categories/i });
    expect(categoriesLink).toHaveClass("bg-primary");
  });

  it("should have correct navigation structure with proper links", () => {
    mockPathname.mockReturnValue("/");

    render(<DashboardNav />);

    expect(screen.getByRole("link", { name: /overview/i })).toHaveAttribute(
      "href",
      "/",
    );
    expect(screen.getByRole("link", { name: /books/i })).toHaveAttribute(
      "href",
      "/books",
    );
    expect(screen.getByRole("link", { name: /categories/i })).toHaveAttribute(
      "href",
      "/categories",
    );
    expect(screen.getByRole("link", { name: /authors/i })).toHaveAttribute(
      "href",
      "/authors",
    );
    expect(screen.getByRole("link", { name: /publishers/i })).toHaveAttribute(
      "href",
      "/publishers",
    );
  });

  it("should have proper accessibility attributes", () => {
    mockPathname.mockReturnValue("/");

    render(<DashboardNav />);

    const nav = screen.getByRole("navigation");
    expect(nav).toHaveAttribute("aria-label", "Main navigation");
  });

  it("should not mark overview as active on other pages", () => {
    mockPathname.mockReturnValue("/books");

    render(<DashboardNav />);

    const overviewLink = screen.getByRole("link", { name: /overview/i });
    expect(overviewLink).not.toHaveClass("bg-primary");
  });
});
