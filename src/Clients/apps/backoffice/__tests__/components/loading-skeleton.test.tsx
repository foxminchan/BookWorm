import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import {
  BookFormSkeleton,
  FilterTableSkeleton,
  OrderDetailSkeleton,
  SimpleTableSkeleton,
} from "@/components/loading-skeleton";

describe("Loading Skeletons", () => {
  describe("OrderDetailSkeleton", () => {
    it("should render loading state with accessibility attributes", () => {
      render(<OrderDetailSkeleton />);

      const status = screen.getByRole("status");
      expect(status).toHaveAttribute("aria-live", "polite");
      expect(status).toHaveAttribute("aria-busy", "true");
      expect(screen.getByText("Loading order details...")).toBeInTheDocument();
      expect(screen.getByText("Loading order details...")).toHaveClass(
        "sr-only",
      );
    });

    it("should render skeleton elements", () => {
      const { container } = render(<OrderDetailSkeleton />);

      // Check for skeleton elements
      const skeletons = container.querySelectorAll('[role="status"]');
      expect(skeletons.length).toBeGreaterThan(0);
    });
  });

  describe("BookFormSkeleton", () => {
    it("should render loading state with accessibility attributes", () => {
      render(<BookFormSkeleton />);

      const status = screen.getByRole("status");
      expect(status).toHaveAttribute("aria-live", "polite");
      expect(status).toHaveAttribute("aria-busy", "true");
      expect(screen.getByText("Loading book form...")).toBeInTheDocument();
      expect(screen.getByText("Loading book form...")).toHaveClass("sr-only");
    });

    it("should render multiple skeleton sections", () => {
      const { container } = render(<BookFormSkeleton />);

      // Should have multiple skeleton elements for form fields
      const skeletons = container.querySelectorAll('[role="status"]');
      expect(skeletons.length).toBeGreaterThan(0);
    });
  });

  describe("FilterTableSkeleton", () => {
    it("should render loading state with accessibility attributes", () => {
      render(
        <FilterTableSkeleton
          title="Test Table"
          description="Test Description"
        />,
      );

      const status = screen.getByRole("status");
      expect(status).toHaveAttribute("aria-live", "polite");
      expect(status).toHaveAttribute("aria-busy", "true");
      expect(screen.getByText("Loading filtered table...")).toBeInTheDocument();
      expect(screen.getByText("Loading filtered table...")).toHaveClass(
        "sr-only",
      );
    });

    it("should render skeleton elements", () => {
      const { container } = render(<FilterTableSkeleton title="Test Table" />);

      // Should have skeleton elements
      const skeletons = container.querySelectorAll('[data-slot="skeleton"]');
      expect(skeletons.length).toBeGreaterThan(0);
    });
  });

  describe("SimpleTableSkeleton", () => {
    it("should render loading state with accessibility attributes", () => {
      render(
        <SimpleTableSkeleton title="Simple Table" description="Description" />,
      );

      const status = screen.getByRole("status");
      expect(status).toHaveAttribute("aria-live", "polite");
      expect(status).toHaveAttribute("aria-busy", "true");
      expect(screen.getByText("Loading table...")).toBeInTheDocument();
      expect(screen.getByText("Loading table...")).toHaveClass("sr-only");
    });

    it("should render skeleton elements", () => {
      const { container } = render(
        <SimpleTableSkeleton title="Simple Table" description="Description" />,
      );

      // Should have skeleton elements
      const skeletons = container.querySelectorAll('[role="status"]');
      expect(skeletons.length).toBeGreaterThan(0);
    });
  });
});
