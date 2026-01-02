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
});
