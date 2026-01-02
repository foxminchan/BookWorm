import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import CategoriesHeader from "@/features/catalog/categories/categories-header";

import { renderWithProviders } from "../../utils/test-utils";

describe("CategoriesHeader", () => {
  it("should display genres heading", () => {
    renderWithProviders(<CategoriesHeader />);

    expect(screen.getByText("Genres")).toBeInTheDocument();
  });

  it("should display description text", () => {
    renderWithProviders(<CategoriesHeader />);

    expect(
      screen.getByText(/from timeless classics to modern discoveries/i),
    ).toBeInTheDocument();
  });

  it("should have serif font for heading", () => {
    renderWithProviders(<CategoriesHeader />);

    const heading = screen.getByText("Genres");
    expect(heading).toHaveClass("font-serif");
  });

  it("should have responsive text sizes", () => {
    renderWithProviders(<CategoriesHeader />);

    const heading = screen.getByText("Genres");
    expect(heading).toHaveClass("text-5xl");
    expect(heading).toHaveClass("md:text-6xl");
  });

  it("should have muted foreground for description", () => {
    renderWithProviders(<CategoriesHeader />);

    const description = screen.getByText(/from timeless classics/i);
    expect(description).toHaveClass("text-muted-foreground");
  });

  it("should have proper spacing", () => {
    const { container } = renderWithProviders(<CategoriesHeader />);

    const wrapper = container.querySelector(".mb-16");
    expect(wrapper).toBeInTheDocument();
  });
});
