import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import ProductInfo from "@/features/catalog/product/product-info";

import { renderWithProviders } from "../../utils/test-utils";

describe("ProductInfo", () => {
  const defaultProps = {
    name: "Test Book",
    authors: [{ name: "John Doe" }, { name: "Jane Smith" }],
    averageRating: 4.5,
    totalReviews: 25,
    price: 29.99,
    status: "InStock",
    category: "Fiction",
    publisher: "Test Publisher",
    description: "This is a test book description that explains the content.",
  };

  it("should display book title", () => {
    renderWithProviders(<ProductInfo {...defaultProps} />);

    expect(
      screen.getByRole("heading", { name: "Test Book" }),
    ).toBeInTheDocument();
  });

  it("should display category", () => {
    renderWithProviders(<ProductInfo {...defaultProps} />);

    const categories = screen.getAllByText("Fiction");
    expect(categories.length).toBeGreaterThan(0);
  });

  it("should display all authors", () => {
    renderWithProviders(<ProductInfo {...defaultProps} />);

    expect(screen.getByText(/John Doe, Jane Smith/)).toBeInTheDocument();
  });

  it("should display average rating", () => {
    renderWithProviders(<ProductInfo {...defaultProps} />);

    expect(screen.getByText("4.5")).toBeInTheDocument();
  });

  it("should display review count", () => {
    renderWithProviders(<ProductInfo {...defaultProps} />);

    expect(screen.getByText("25 Reviews")).toBeInTheDocument();
  });

  it("should display regular price when no sale", () => {
    renderWithProviders(<ProductInfo {...defaultProps} />);

    expect(screen.getByText("$29.99")).toBeInTheDocument();
  });

  it("should display sale price with strikethrough original price", () => {
    renderWithProviders(<ProductInfo {...defaultProps} priceSale={19.99} />);

    expect(screen.getByText("$19.99")).toBeInTheDocument();
    expect(screen.getByText("$29.99")).toBeInTheDocument();
  });

  it("should display in stock status", () => {
    renderWithProviders(<ProductInfo {...defaultProps} />);

    expect(screen.getByText("Available in Store")).toBeInTheDocument();
  });

  it("should display out of stock status", () => {
    renderWithProviders(<ProductInfo {...defaultProps} status="OutOfStock" />);

    expect(screen.getByText("Out of Stock")).toBeInTheDocument();
  });

  it("should display description", () => {
    renderWithProviders(<ProductInfo {...defaultProps} />);

    expect(
      screen.getByText(/This is a test book description/),
    ).toBeInTheDocument();
  });

  it("should display publisher", () => {
    renderWithProviders(<ProductInfo {...defaultProps} />);

    expect(screen.getByText("Test Publisher")).toBeInTheDocument();
  });

  it("should render 5 star icons", () => {
    const { container } = renderWithProviders(
      <ProductInfo {...defaultProps} />,
    );

    const stars = container.querySelectorAll("svg");
    const starIcons = Array.from(stars).filter((svg) =>
      svg.classList.contains("lucide-star"),
    );
    expect(starIcons.length).toBeGreaterThanOrEqual(5);
  });
});
