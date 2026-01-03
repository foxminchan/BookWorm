import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import CategoriesGrid from "@/features/catalog/categories/categories-grid";

import { renderWithProviders } from "../../utils/test-utils";

type Category = {
  id: string;
  name: string | null;
};

const mockCategories: Category[] = [
  { id: faker.string.uuid(), name: faker.commerce.department() },
  { id: faker.string.uuid(), name: faker.commerce.department() },
  { id: faker.string.uuid(), name: faker.commerce.department() },
  { id: faker.string.uuid(), name: faker.commerce.department() },
];

describe("CategoriesGrid", () => {
  it("should render all categories", () => {
    const categories = [
      { ...mockCategories[0]!, name: "Fiction" },
      { ...mockCategories[1]!, name: "Non-Fiction" },
      { ...mockCategories[2]!, name: "Science Fiction" },
      { ...mockCategories[3]!, name: "Fantasy" },
    ];

    renderWithProviders(
      <CategoriesGrid categories={categories} isLoading={false} />,
    );

    expect(screen.getByText("Fiction")).toBeInTheDocument();
    expect(screen.getByText("Non-Fiction")).toBeInTheDocument();
    expect(screen.getByText("Science Fiction")).toBeInTheDocument();
    expect(screen.getByText("Fantasy")).toBeInTheDocument();
  });

  it("should render loading skeletons when loading", () => {
    const { container } = renderWithProviders(
      <CategoriesGrid categories={[]} isLoading={true} />,
    );

    // Should render 8 skeleton cards
    const skeletons = container.querySelectorAll(".animate-pulse");
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("should not render categories when loading", () => {
    renderWithProviders(
      <CategoriesGrid categories={mockCategories} isLoading={true} />,
    );

    expect(screen.queryByText("Fiction")).not.toBeInTheDocument();
  });

  it("should render empty grid when no categories", () => {
    const { container } = renderWithProviders(
      <CategoriesGrid categories={[]} isLoading={false} />,
    );

    const grid = container.querySelector(".grid");
    expect(grid).toBeInTheDocument();
    expect(grid?.children).toHaveLength(0);
  });

  it("should display 'Unknown Category' for null names", () => {
    const categoriesWithNull: Category[] = [
      { ...mockCategories[0]!, name: null },
      { ...mockCategories[1]!, name: "Fiction" },
    ];

    renderWithProviders(
      <CategoriesGrid categories={categoriesWithNull} isLoading={false} />,
    );

    expect(screen.getByText("Unknown Category")).toBeInTheDocument();
    expect(screen.getByText("Fiction")).toBeInTheDocument();
  });

  it("should render grid with proper styling", () => {
    const { container } = renderWithProviders(
      <CategoriesGrid categories={mockCategories} isLoading={false} />,
    );

    const grid = container.querySelector(".grid");
    expect(grid).toHaveClass("grid-cols-1");
  });

  it("should have responsive grid layout", () => {
    const { container } = renderWithProviders(
      <CategoriesGrid categories={mockCategories} isLoading={false} />,
    );

    const grid = container.querySelector(".grid");
    expect(grid).toHaveClass("md:grid-cols-2");
  });

  it("should render links for all categories", () => {
    renderWithProviders(
      <CategoriesGrid categories={mockCategories} isLoading={false} />,
    );

    const links = screen.getAllByRole("link");
    expect(links).toHaveLength(4);
  });

  it("should create proper category filter links", () => {
    const categories = [{ ...mockCategories[0]!, id: "cat-1" }];
    renderWithProviders(
      <CategoriesGrid categories={categories} isLoading={false} />,
    );

    const link = screen.getByRole("link");
    expect(link).toHaveAttribute("href", "/shop?category=cat-1");
  });

  it("should handle single category", () => {
    const categories = [{ ...mockCategories[0]!, name: "Fiction" }];
    renderWithProviders(
      <CategoriesGrid categories={categories} isLoading={false} />,
    );

    expect(screen.getByText("Fiction")).toBeInTheDocument();
    const links = screen.getAllByRole("link");
    expect(links).toHaveLength(1);
  });

  it("should have border styling", () => {
    const { container } = renderWithProviders(
      <CategoriesGrid categories={mockCategories} isLoading={false} />,
    );

    const grid = container.querySelector(".border");
    expect(grid).toBeInTheDocument();
  });

  it("should render with gap between items", () => {
    const { container } = renderWithProviders(
      <CategoriesGrid categories={mockCategories} isLoading={false} />,
    );

    const grid = container.querySelector(".gap-px");
    expect(grid).toBeInTheDocument();
  });
});
