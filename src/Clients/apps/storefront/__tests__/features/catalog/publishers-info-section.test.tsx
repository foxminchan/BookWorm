import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import PublishersInfoSection from "@/features/catalog/publisher/publishers-info-section";

import { renderWithProviders } from "../../utils/test-utils";

describe("PublishersInfoSection", () => {
  it("should display 'Why Publishers Matter' heading", () => {
    renderWithProviders(<PublishersInfoSection />);

    expect(screen.getByText("Why Publishers Matter")).toBeInTheDocument();
  });

  it("should display 'Find Your Next Read' heading", () => {
    renderWithProviders(<PublishersInfoSection />);

    expect(screen.getByText("Find Your Next Read")).toBeInTheDocument();
  });

  it("should display description for why publishers matter", () => {
    renderWithProviders(<PublishersInfoSection />);

    expect(
      screen.getByText(/Publishing houses are more than distributors/),
    ).toBeInTheDocument();
  });

  it("should display description for find your next read", () => {
    renderWithProviders(<PublishersInfoSection />);

    expect(
      screen.getByText(/Browse books from your favorite publishers/),
    ).toBeInTheDocument();
  });

  it("should render two info sections", () => {
    renderWithProviders(<PublishersInfoSection />);

    const headings = screen.getAllByRole("heading", { level: 3 });
    expect(headings).toHaveLength(2);
  });
});
