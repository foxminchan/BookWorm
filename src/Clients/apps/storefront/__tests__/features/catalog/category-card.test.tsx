import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import CategoryCard from "@/features/catalog/categories/category-card";

import { renderWithProviders } from "../../utils/test-utils";

describe("CategoryCard", () => {
  const mockCategory = {
    id: faker.string.uuid(),
    name: faker.commerce.department(),
  };

  it("should render category name", () => {
    const category = { ...mockCategory, name: "Science Fiction" };
    renderWithProviders(<CategoryCard id={category.id} name={category.name} />);

    expect(screen.getByText("Science Fiction")).toBeInTheDocument();
  });

  it("should display genre label", () => {
    renderWithProviders(
      <CategoryCard id={mockCategory.id} name={mockCategory.name} />,
    );

    expect(screen.getByText("Genre")).toBeInTheDocument();
  });

  it("should render link to shop with category filter", () => {
    renderWithProviders(
      <CategoryCard id={mockCategory.id} name={mockCategory.name} />,
    );

    const link = screen.getByRole("link");
    expect(link).toHaveAttribute("href", `/shop?category=${mockCategory.id}`);
  });

  it("should display arrow icon", () => {
    const { container } = renderWithProviders(
      <CategoryCard id={mockCategory.id} name={mockCategory.name} />,
    );

    const arrowIcon = container.querySelector("svg");
    expect(arrowIcon).toBeInTheDocument();
  });

  it("should have hover effects", () => {
    const { container } = renderWithProviders(
      <CategoryCard id={mockCategory.id} name={mockCategory.name} />,
    );

    const card = container.querySelector(".group");
    expect(card).toHaveClass("hover:bg-secondary/20");
  });

  it("should apply transition classes", () => {
    const { container } = renderWithProviders(
      <CategoryCard id={mockCategory.id} name={mockCategory.name} />,
    );

    const card = container.querySelector(".group");
    expect(card).toHaveClass("transition-all", "duration-300");
  });

  it("should render title with proper typography", () => {
    const category = { ...mockCategory, name: "Science Fiction" };
    renderWithProviders(<CategoryCard id={category.id} name={category.name} />);

    const title = screen.getByText("Science Fiction");
    expect(title).toHaveClass("font-serif");
  });

  it("should have circular arrow container", () => {
    const { container } = renderWithProviders(
      <CategoryCard id={mockCategory.id} name={mockCategory.name} />,
    );

    const arrowContainer = container.querySelector(".rounded-full");
    expect(arrowContainer).toBeInTheDocument();
  });

  it("should handle different category names", () => {
    renderWithProviders(<CategoryCard id="cat-456" name="Mystery" />);

    expect(screen.getByText("Mystery")).toBeInTheDocument();
  });

  it("should format long category names", () => {
    const longName = "Historical Fiction and Literature";
    renderWithProviders(<CategoryCard id="cat-789" name={longName} />);

    expect(screen.getByText(longName)).toBeInTheDocument();
  });

  it("should have proper link accessibility", () => {
    renderWithProviders(
      <CategoryCard id={mockCategory.id} name={mockCategory.name} />,
    );

    const link = screen.getByRole("link");
    expect(link).toBeInTheDocument();
  });

  it("should display genre label in uppercase", () => {
    renderWithProviders(
      <CategoryCard id={mockCategory.id} name={mockCategory.name} />,
    );

    const genreLabel = screen.getByText("Genre");
    expect(genreLabel).toHaveClass("uppercase");
  });
});
