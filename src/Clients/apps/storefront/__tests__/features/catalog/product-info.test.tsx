import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import ProductInfo from "@/features/catalog/product/product-info";

import { renderWithProviders } from "../../utils/test-utils";

describe("ProductInfo", () => {
  const defaultProps = {
    name: faker.commerce.productName(),
    authors: [
      { name: faker.person.fullName() },
      { name: faker.person.fullName() },
    ],
    averageRating: faker.number.float({ min: 1, max: 5, fractionDigits: 1 }),
    totalReviews: faker.number.int({ min: 1, max: 100 }),
    price: faker.number.float({ min: 10, max: 50, fractionDigits: 2 }),
    status: "InStock",
    category: faker.commerce.department(),
    publisher: faker.company.name(),
    description: faker.commerce.productDescription(),
  };

  it("should display book title", () => {
    const props = { ...defaultProps, name: "Test Book" };
    renderWithProviders(<ProductInfo {...props} />);

    expect(
      screen.getByRole("heading", { name: "Test Book" }),
    ).toBeInTheDocument();
  });

  it("should display category", () => {
    const props = { ...defaultProps, category: "Fiction" };
    renderWithProviders(<ProductInfo {...props} />);

    const categories = screen.getAllByText("Fiction");
    expect(categories.length).toBeGreaterThan(0);
  });

  it("should display all authors", () => {
    const props = {
      ...defaultProps,
      authors: [{ name: "John Doe" }, { name: "Jane Smith" }],
    };
    renderWithProviders(<ProductInfo {...props} />);

    expect(screen.getByText(/John Doe, Jane Smith/)).toBeInTheDocument();
  });

  it("should display average rating", () => {
    const props = { ...defaultProps, averageRating: 4.5 };
    renderWithProviders(<ProductInfo {...props} />);

    expect(screen.getByText("4.5")).toBeInTheDocument();
  });

  it("should display review count", () => {
    const props = { ...defaultProps, totalReviews: 25 };
    renderWithProviders(<ProductInfo {...props} />);

    expect(screen.getByText("25 Reviews")).toBeInTheDocument();
  });

  it("should display regular price when no sale", () => {
    const props = { ...defaultProps, price: 29.99 };
    renderWithProviders(<ProductInfo {...props} />);

    expect(screen.getByText("$29.99")).toBeInTheDocument();
  });

  it("should display sale price with strikethrough original price", () => {
    const props = { ...defaultProps, price: 29.99 };
    renderWithProviders(<ProductInfo {...props} priceSale={19.99} />);

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
    const props = {
      ...defaultProps,
      description: "This is a test book description that explains the content.",
    };
    renderWithProviders(<ProductInfo {...props} />);

    expect(
      screen.getByText(/This is a test book description/),
    ).toBeInTheDocument();
  });

  it("should display publisher", () => {
    const props = { ...defaultProps, publisher: "Test Publisher" };
    renderWithProviders(<ProductInfo {...props} />);

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
