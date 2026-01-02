import { screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import ProductImage from "@/features/catalog/product/product-image";

import { renderWithProviders } from "../../utils/test-utils";

// Mock next/image to avoid URL validation issues in test environment
vi.mock("next/image", () => ({
  default: (props: any) => {
    // eslint-disable-next-line @next/next/no-img-element, jsx-a11y/alt-text
    return <img {...props} />;
  },
}));

describe("ProductImage", () => {
  it("should display book image", () => {
    renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" hasSale={false} />,
    );

    const image = screen.getByRole("img");
    expect(image).toBeInTheDocument();
  });

  it("should display sale badge when hasSale is true", () => {
    renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" hasSale={true} />,
    );

    expect(screen.getByText("SALE")).toBeInTheDocument();
  });

  it("should not display sale badge when hasSale is false", () => {
    renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" hasSale={false} />,
    );

    expect(screen.queryByText("SALE")).not.toBeInTheDocument();
  });

  it("should have figure element", () => {
    const { container } = renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" />,
    );

    const figure = container.querySelector("figure");
    expect(figure).toBeInTheDocument();
  });

  it("should have correct styling classes", () => {
    const { container } = renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" />,
    );

    const figure = container.querySelector("figure");
    expect(figure).toHaveClass("group", "relative");
  });

  it("should have correct alt text", () => {
    renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" hasSale={false} />,
    );

    const image = screen.getByRole("img");
    expect(image).toHaveAttribute("alt", "Test Book book cover");
  });

  it("should use default image when imageUrl is not provided", () => {
    renderWithProviders(<ProductImage name="Test Book" hasSale={false} />);

    const image = screen.getByRole("img");
    expect(image).toBeInTheDocument();
  });

  it("should have aspect ratio styling", () => {
    const { container } = renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" />,
    );

    const figure = container.querySelector("figure");
    expect(figure).toHaveClass("aspect-3/4");
  });

  it("should have rounded corners", () => {
    const { container } = renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" />,
    );

    const figure = container.querySelector("figure");
    expect(figure).toHaveClass("rounded-2xl");
  });

  it("should have overflow hidden", () => {
    const { container } = renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" />,
    );

    const figure = container.querySelector("figure");
    expect(figure).toHaveClass("overflow-hidden");
  });

  it("should have shadow", () => {
    const { container } = renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" />,
    );

    const figure = container.querySelector("figure");
    expect(figure).toHaveClass("shadow-sm");
  });

  it("should have secondary background", () => {
    const { container } = renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" />,
    );

    const figure = container.querySelector("figure");
    expect(figure).toHaveClass("bg-secondary");
  });

  it("should have sale badge with correct styling", () => {
    const { container } = renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" hasSale={true} />,
    );

    const badge = container.querySelector(".bg-destructive");
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass("absolute", "top-6", "left-6", "z-10");
  });

  it("should render with itemProp for schema.org", () => {
    renderWithProviders(<ProductImage imageUrl="/book.jpg" name="Test Book" />);

    const image = screen.getByRole("img");
    expect(image).toHaveAttribute("itemprop", "image");
  });

  it("should handle different book names", () => {
    renderWithProviders(
      <ProductImage
        imageUrl="/book.jpg"
        name="The Great Gatsby"
        hasSale={false}
      />,
    );

    const image = screen.getByRole("img");
    expect(image).toHaveAttribute("alt", "The Great Gatsby book cover");
  });

  it("should have transition effect on image", () => {
    renderWithProviders(<ProductImage imageUrl="/book.jpg" name="Test Book" />);

    const image = screen.getByRole("img");
    expect(image).toHaveClass("transition-transform", "duration-700");
  });

  it("should have hover scale effect", () => {
    renderWithProviders(<ProductImage imageUrl="/book.jpg" name="Test Book" />);

    const image = screen.getByRole("img");
    expect(image).toHaveClass("group-hover:scale-105");
  });
});
