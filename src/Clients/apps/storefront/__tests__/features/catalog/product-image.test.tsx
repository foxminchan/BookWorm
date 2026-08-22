import { screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import ProductImage from "@/features/catalog/product/product-image";

import { renderWithProviders } from "../../utils/test-utils";

// Mock next/image to avoid URL validation issues in test environment
vi.mock("next/image", () => ({
  default: (props: any) => {
    // Filter out Next.js-specific props that shouldn't be passed to img element
    const { fill, priority, quality, sizes, loader, ...imgProps } = props;
    // eslint-disable-next-line @next/next/no-img-element
    return <img alt="" {...imgProps} />;
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

  it.each([
    ["sale badge when hasSale is true", true, "SALE"],
    ["no sale badge when hasSale is false", false, null],
  ])("should %s", (_label, hasSale, expectedText) => {
    renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" hasSale={hasSale} />,
    );

    if (expectedText) {
      expect(screen.getByText(expectedText)).toBeInTheDocument();
      return;
    }

    expect(screen.queryByText("SALE")).not.toBeInTheDocument();
  });

  it.each([
    ["figure element", "figure", undefined],
    ["correct styling classes", "figure", ["group", "relative"]],
    ["aspect ratio styling", "figure", ["aspect-3/4"]],
    ["rounded corners", "figure", ["rounded-2xl"]],
    ["overflow hidden", "figure", ["overflow-hidden"]],
    ["shadow", "figure", ["shadow-sm"]],
    ["secondary background", "figure", ["bg-secondary"]],
  ])("should have %s", (_label, selector, classNames) => {
    const { container } = renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name="Test Book" />,
    );

    const element = container.querySelector(selector);

    if (classNames) {
      expect(element).toHaveClass(...classNames);
      return;
    }

    expect(element).toBeInTheDocument();
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

  it.each([
    ["The Great Gatsby", "The Great Gatsby book cover"],
    ["Test Book", "Test Book book cover"],
  ])("should handle book name %s", (name, expectedAlt) => {
    renderWithProviders(
      <ProductImage imageUrl="/book.jpg" name={name} hasSale={false} />,
    );

    const image = screen.getByRole("img");
    expect(image).toHaveAttribute("alt", expectedAlt);
  });

  it.each([
    [
      "transition effect on image",
      "img",
      ["transition-transform", "duration-700"],
    ],
    ["hover scale effect", "img", ["group-hover:scale-105"]],
  ])("should have %s", (_label, selector, classNames) => {
    renderWithProviders(<ProductImage imageUrl="/book.jpg" name="Test Book" />);

    const element = screen.getByRole(selector === "img" ? "img" : "figure");
    expect(element).toHaveClass(...classNames);
  });
});
