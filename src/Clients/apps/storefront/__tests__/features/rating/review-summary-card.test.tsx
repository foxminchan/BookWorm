import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import ReviewSummaryCard from "@/features/catalog/product/review-summary-card";

import { renderWithProviders } from "../../utils/test-utils";

describe("ReviewSummaryCard", () => {
  it("should display average rating", () => {
    renderWithProviders(
      <ReviewSummaryCard averageRating={4.5} totalReviews={100} />,
    );

    expect(screen.getByText("4.5")).toBeInTheDocument();
  });

  it("should display total reviews count", () => {
    renderWithProviders(
      <ReviewSummaryCard averageRating={4.5} totalReviews={100} />,
    );

    expect(screen.getByText(/based on 100 reviews/i)).toBeInTheDocument();
  });

  it("should render 5 stars", () => {
    const { container } = renderWithProviders(
      <ReviewSummaryCard averageRating={4.5} totalReviews={100} />,
    );

    const stars = container.querySelectorAll("svg");
    expect(stars).toHaveLength(5);
  });

  it("should fill stars according to rating", () => {
    const { container } = renderWithProviders(
      <ReviewSummaryCard averageRating={3.7} totalReviews={50} />,
    );

    // 3 filled stars (floor of 3.7)
    const filledStars = container.querySelectorAll(".fill-primary");
    expect(filledStars).toHaveLength(3);
  });

  it("should format rating to one decimal place", () => {
    renderWithProviders(
      <ReviewSummaryCard averageRating={4.567} totalReviews={100} />,
    );

    expect(screen.getByText("4.6")).toBeInTheDocument();
  });

  it("should display zero rating", () => {
    renderWithProviders(
      <ReviewSummaryCard averageRating={0} totalReviews={0} />,
    );

    expect(screen.getByText("0.0")).toBeInTheDocument();
  });

  it("should display single review correctly", () => {
    renderWithProviders(
      <ReviewSummaryCard averageRating={5} totalReviews={1} />,
    );

    expect(screen.getByText(/based on 1 reviews/i)).toBeInTheDocument();
  });

  it("should have rounded corners", () => {
    const { container } = renderWithProviders(
      <ReviewSummaryCard averageRating={4.5} totalReviews={100} />,
    );

    const card = container.querySelector(".rounded-2xl");
    expect(card).toBeInTheDocument();
  });

  it("should have centered text", () => {
    const { container } = renderWithProviders(
      <ReviewSummaryCard averageRating={4.5} totalReviews={100} />,
    );

    const card = container.querySelector(".text-center");
    expect(card).toBeInTheDocument();
  });

  it("should display rating with serif font", () => {
    renderWithProviders(
      <ReviewSummaryCard averageRating={4.5} totalReviews={100} />,
    );

    const rating = screen.getByText("4.5");
    expect(rating).toHaveClass("font-serif");
  });

  it("should handle maximum rating", () => {
    const { container } = renderWithProviders(
      <ReviewSummaryCard averageRating={5} totalReviews={200} />,
    );

    expect(screen.getByText("5.0")).toBeInTheDocument();
    const filledStars = container.querySelectorAll(".fill-primary");
    expect(filledStars).toHaveLength(5);
  });

  it("should handle large review counts", () => {
    renderWithProviders(
      <ReviewSummaryCard averageRating={4.2} totalReviews={15000} />,
    );

    expect(screen.getByText(/based on 15000 reviews/i)).toBeInTheDocument();
  });
});
