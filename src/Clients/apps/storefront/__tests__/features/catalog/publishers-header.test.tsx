import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import PublishersHeader from "@/features/catalog/publisher/publishers-header";

import { renderWithProviders } from "../../utils/test-utils";

describe("PublishersHeader", () => {
  it("should display heading", () => {
    renderWithProviders(<PublishersHeader />);

    expect(
      screen.getByText(/discover premier publishing houses/i),
    ).toBeInTheDocument();
  });

  it("should display description text", () => {
    renderWithProviders(<PublishersHeader />);

    expect(
      screen.getByText(/explore the world's most respected publishers/i),
    ).toBeInTheDocument();
  });

  it("should have serif font for heading", () => {
    renderWithProviders(<PublishersHeader />);

    const heading = screen.getByText(/discover premier publishing houses/i);
    expect(heading).toHaveClass("font-serif");
  });

  it("should have large responsive text sizes", () => {
    renderWithProviders(<PublishersHeader />);

    const heading = screen.getByText(/discover premier publishing houses/i);
    expect(heading).toHaveClass("text-6xl");
    expect(heading).toHaveClass("md:text-7xl");
  });

  it("should have muted foreground for description", () => {
    renderWithProviders(<PublishersHeader />);

    const description = screen.getByText(/explore the world's most respected/i);
    expect(description).toHaveClass("text-muted-foreground");
  });

  it("should have light font weight", () => {
    renderWithProviders(<PublishersHeader />);

    const heading = screen.getByText(/discover premier publishing houses/i);
    expect(heading).toHaveClass("font-light");
  });

  it("should have proper bottom spacing", () => {
    const { container } = renderWithProviders(<PublishersHeader />);

    const wrapper = container.querySelector(".mb-20");
    expect(wrapper).toBeInTheDocument();
  });
});
