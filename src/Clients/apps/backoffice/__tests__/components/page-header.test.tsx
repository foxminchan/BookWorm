import { describe, expect, it } from "vitest";

import { renderWithProviders, screen } from "@/__tests__/utils/test-utils";
import { PageHeader } from "@/components/page-header";

describe("PageHeader", () => {
  it("should render title and breadcrumbs", () => {
    renderWithProviders(
      <PageHeader
        title="Books Management"
        breadcrumbs={[
          { label: "Home", href: "/" },
          { label: "Books", isActive: true },
        ]}
      />,
    );

    expect(screen.getByText("Books Management")).toBeInTheDocument();
    expect(screen.getByText("Home")).toBeInTheDocument();
    expect(screen.getByText("Books")).toBeInTheDocument();
  });

  it("should render description when provided", () => {
    renderWithProviders(
      <PageHeader
        title="Books"
        description="Manage your book collection"
        breadcrumbs={[{ label: "Books", isActive: true }]}
      />,
    );

    expect(screen.getByText("Manage your book collection")).toBeInTheDocument();
  });

  it("should not render description when not provided", () => {
    const { container } = renderWithProviders(
      <PageHeader
        title="Books"
        breadcrumbs={[{ label: "Books", isActive: true }]}
      />,
    );

    const description = container.querySelector("p");
    expect(description).not.toBeInTheDocument();
  });

  it("should render action button when provided", () => {
    renderWithProviders(
      <PageHeader
        title="Books"
        breadcrumbs={[{ label: "Books", isActive: true }]}
        action={<button type="button">Add Book</button>}
      />,
    );

    expect(screen.getByText("Add Book")).toBeInTheDocument();
  });

  it("should render multiple breadcrumbs with separators", () => {
    renderWithProviders(
      <PageHeader
        title="Edit Book"
        breadcrumbs={[
          { label: "Home", href: "/" },
          { label: "Books", href: "/books" },
          { label: "Edit", isActive: true },
        ]}
      />,
    );

    expect(screen.getByText("Home")).toBeInTheDocument();
    expect(screen.getByText("Books")).toBeInTheDocument();
    expect(screen.getByText("Edit")).toBeInTheDocument();
  });

  it("should render active breadcrumb as page instead of link", () => {
    renderWithProviders(
      <PageHeader
        title="Categories"
        breadcrumbs={[
          { label: "Home", href: "/" },
          { label: "Categories", isActive: true },
        ]}
      />,
    );

    const homeLink = screen.getByText("Home").closest("a");
    expect(homeLink).toHaveAttribute("href", "/");

    // Get breadcrumb page element (not the title h1)
    const breadcrumbNav = screen.getByRole("navigation", {
      name: "Breadcrumb",
    });
    const categoriesPage = breadcrumbNav.querySelector(
      '[data-slot="breadcrumb-page"]',
    );
    expect(categoriesPage).toHaveTextContent("Categories");
    expect(categoriesPage?.closest("a")).not.toBeInTheDocument();
  });

  it("should have proper ARIA label for breadcrumb navigation", () => {
    renderWithProviders(
      <PageHeader
        title="Books"
        breadcrumbs={[{ label: "Books", isActive: true }]}
      />,
    );

    const nav = screen.getByRole("navigation", { name: "Breadcrumb" });
    expect(nav).toBeInTheDocument();
  });
});
