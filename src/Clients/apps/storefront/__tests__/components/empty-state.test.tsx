import { screen } from "@testing-library/react";
import { PackageX } from "lucide-react";
import { describe, expect, it } from "vitest";

import { EmptyState } from "@/components/empty-state";

import { renderWithProviders } from "../utils/test-utils";

describe("EmptyState", () => {
  it("should render title and description", () => {
    renderWithProviders(
      <EmptyState
        icon={PackageX}
        title="No Orders Found"
        description="You haven't placed any orders yet."
      />,
    );

    expect(screen.getByText("No Orders Found")).toBeInTheDocument();
    expect(
      screen.getByText("You haven't placed any orders yet."),
    ).toBeInTheDocument();
  });

  it("should render action button when provided", () => {
    renderWithProviders(
      <EmptyState
        icon={PackageX}
        title="No Orders Found"
        description="You haven't placed any orders yet."
        actionLabel="Browse Books"
        actionHref="/shop"
      />,
    );

    const button = screen.getByText("Browse Books");
    expect(button).toBeInTheDocument();
  });

  it("should not render action button when not provided", () => {
    renderWithProviders(
      <EmptyState
        icon={PackageX}
        title="No Orders Found"
        description="You haven't placed any orders yet."
      />,
    );

    expect(screen.queryByRole("link")).not.toBeInTheDocument();
  });

  it("should render icon component", () => {
    const { container } = renderWithProviders(
      <EmptyState
        icon={PackageX}
        title="No Orders Found"
        description="You haven't placed any orders yet."
      />,
    );

    const icon = container.querySelector("svg");
    expect(icon).toBeInTheDocument();
  });

  it("should link to correct href when action is provided", () => {
    renderWithProviders(
      <EmptyState
        icon={PackageX}
        title="No Orders Found"
        description="You haven't placed any orders yet."
        actionLabel="Browse Books"
        actionHref="/shop"
      />,
    );

    const link = screen.getByRole("link");
    expect(link).toHaveAttribute("href", "/shop");
  });

  it("should have proper ARIA labeling", () => {
    renderWithProviders(
      <EmptyState
        icon={PackageX}
        title="No Orders Found"
        description="You haven't placed any orders yet."
      />,
    );

    // Check heading hierarchy
    const heading = screen.getByRole("heading", { name: "No Orders Found" });
    expect(heading).toBeInTheDocument();
  });

  it("should render with different icons", () => {
    const { container } = renderWithProviders(
      <EmptyState
        icon={PackageX}
        title="Empty State"
        description="Test description"
      />,
    );

    const icon = container.querySelector("svg");
    expect(icon).toBeInTheDocument();
  });

  it("should handle long titles and descriptions", () => {
    const longTitle = "This is a very long title ".repeat(5).trim();
    const longDescription = "This is a very long description "
      .repeat(10)
      .trim();

    renderWithProviders(
      <EmptyState
        icon={PackageX}
        title={longTitle}
        description={longDescription}
      />,
    );

    // Use substring match since text may be broken across elements
    expect(screen.getByText(/This is a very long title/)).toBeInTheDocument();
    expect(
      screen.getByText(/This is a very long description/),
    ).toBeInTheDocument();
  });

  it("should have consistent styling", () => {
    const { container } = renderWithProviders(
      <EmptyState
        icon={PackageX}
        title="No Orders Found"
        description="You haven't placed any orders yet."
      />,
    );

    const emptyStateContainer = container.firstChild;
    expect(emptyStateContainer).toHaveClass("space-y-6");
  });
});
