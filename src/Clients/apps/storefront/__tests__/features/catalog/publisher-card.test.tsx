import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import PublisherCard from "@/features/catalog/publisher/publisher-card";

import { renderWithProviders } from "../../utils/test-utils";

describe("PublisherCard", () => {
  const mockPublisher = {
    id: faker.string.uuid(),
    name: faker.company.name(),
  };

  it("should render publisher name", () => {
    const publisher = { ...mockPublisher, name: "Penguin Random House" };
    renderWithProviders(
      <PublisherCard id={publisher.id} name={publisher.name} />,
    );

    expect(screen.getByText("Penguin Random House")).toBeInTheDocument();
  });

  it("should display publisher label", () => {
    renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    expect(screen.getByText("Publisher")).toBeInTheDocument();
  });

  it("should render link to shop with publisher filter", () => {
    renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    const link = screen.getByRole("link");
    expect(link).toHaveAttribute("href", `/shop?publisher=${mockPublisher.id}`);
  });

  it("should display explore text", () => {
    renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    expect(screen.getByText("Explore")).toBeInTheDocument();
  });

  it("should display arrow icon", () => {
    const { container } = renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    const arrowIcon = container.querySelector("svg");
    expect(arrowIcon).toBeInTheDocument();
  });

  it("should have hover effects", () => {
    const { container } = renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    const card = container.querySelector(".group");
    expect(card).toHaveClass("hover:shadow-lg");
  });

  it("should apply transition classes", () => {
    const { container } = renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    const card = container.querySelector(".group");
    expect(card).toHaveClass("transition-all", "duration-500");
  });

  it("should render with rounded corners", () => {
    const { container } = renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    const card = container.querySelector(".rounded-lg");
    expect(card).toBeInTheDocument();
  });

  it("should have aspect-square ratio", () => {
    const { container } = renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    const card = container.querySelector(".aspect-square");
    expect(card).toBeInTheDocument();
  });

  it("should render publisher badge with proper styling", () => {
    renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    const badge = screen.getByText("Publisher");
    expect(badge).toHaveClass("uppercase");
  });

  it("should handle different publisher names", () => {
    renderWithProviders(<PublisherCard id="pub-456" name="HarperCollins" />);

    expect(screen.getByText("HarperCollins")).toBeInTheDocument();
  });

  it("should format long publisher names", () => {
    const longName = "Oxford University Press International";
    renderWithProviders(<PublisherCard id="pub-789" name={longName} />);

    expect(screen.getByText(longName)).toBeInTheDocument();
  });

  it("should have proper link accessibility", () => {
    renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    const link = screen.getByRole("link");
    expect(link).toBeInTheDocument();
  });

  it("should render title with serif font", () => {
    const publisher = { ...mockPublisher, name: "Penguin Random House" };
    renderWithProviders(
      <PublisherCard id={publisher.id} name={publisher.name} />,
    );

    const title = screen.getByText("Penguin Random House");
    expect(title).toHaveClass("font-serif");
  });

  it("should have gradient background", () => {
    const { container } = renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    const card = container.querySelector(".bg-linear-to-br");
    expect(card).toBeInTheDocument();
  });

  it("should have overlay effect", () => {
    const { container } = renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    const overlay = container.querySelector(".absolute.inset-0");
    expect(overlay).toBeInTheDocument();
  });
});
