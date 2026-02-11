import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import {
  BasketItemSkeleton,
  BookCardSkeleton,
  CategoryCardSkeleton,
  PublisherCardSkeleton,
} from "@/components/loading-skeleton";

import { renderWithProviders } from "../utils/test-utils";

describe("Loading Skeletons", () => {
  describe("BookCardSkeleton", () => {
    it("should render with loading aria-label", () => {
      renderWithProviders(<BookCardSkeleton />);

      expect(screen.getByLabelText("Loading book card")).toBeInTheDocument();
    });

    it("should include screen reader text", () => {
      renderWithProviders(<BookCardSkeleton />);

      expect(
        screen.getByText("Loading book information..."),
      ).toBeInTheDocument();
    });
  });

  describe("CategoryCardSkeleton", () => {
    it("should render with loading aria-label", () => {
      renderWithProviders(<CategoryCardSkeleton />);

      expect(screen.getByLabelText("Loading category")).toBeInTheDocument();
    });

    it("should include screen reader text", () => {
      renderWithProviders(<CategoryCardSkeleton />);

      expect(
        screen.getByText("Loading category information..."),
      ).toBeInTheDocument();
    });
  });

  describe("PublisherCardSkeleton", () => {
    it("should render with loading aria-label", () => {
      renderWithProviders(<PublisherCardSkeleton />);

      expect(screen.getByLabelText("Loading publisher")).toBeInTheDocument();
    });

    it("should include screen reader text", () => {
      renderWithProviders(<PublisherCardSkeleton />);

      expect(
        screen.getByText("Loading publisher information..."),
      ).toBeInTheDocument();
    });
  });

  describe("BasketItemSkeleton", () => {
    it("should render with loading aria-label", () => {
      renderWithProviders(<BasketItemSkeleton />);

      expect(screen.getByLabelText("Loading basket item")).toBeInTheDocument();
    });

    it("should include screen reader text", () => {
      renderWithProviders(<BasketItemSkeleton />);

      expect(screen.getByText("Loading basket item...")).toBeInTheDocument();
    });
  });
});
