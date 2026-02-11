import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import HeroSection from "@/features/home/hero-section";

import { renderWithProviders } from "../../utils/test-utils";

describe("HeroSection", () => {
  it("should render the hero heading", () => {
    renderWithProviders(<HeroSection />);

    expect(
      screen.getByText("Great stories await your next chapter."),
    ).toBeInTheDocument();
  });

  it("should render the hero description", () => {
    renderWithProviders(<HeroSection />);

    expect(
      screen.getByText(
        /Discover a curated collection of literature, design, and inspiration/,
      ),
    ).toBeInTheDocument();
  });

  it("should have a link to browse collection", () => {
    renderWithProviders(<HeroSection />);

    const link = screen.getByRole("link", { name: /browse book collection/i });
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute("href", "/shop");
  });

  it("should have proper aria-labelledby on section", () => {
    renderWithProviders(<HeroSection />);

    const heading = screen.getByRole("heading", { level: 1 });
    expect(heading).toHaveAttribute("id", "hero-heading");
  });

  it("should render as a section element with aria-labelledby", () => {
    const { container } = renderWithProviders(<HeroSection />);

    const section = container.querySelector(
      "section[aria-labelledby='hero-heading']",
    );
    expect(section).toBeTruthy();
  });
});
