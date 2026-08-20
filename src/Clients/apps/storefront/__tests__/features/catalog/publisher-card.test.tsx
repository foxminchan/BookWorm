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

  it.each([
    ["Penguin Random House", "Penguin Random House"],
    ["HarperCollins", "HarperCollins"],
    [
      "Oxford University Press International",
      "Oxford University Press International",
    ],
  ])("should render publisher name %s", (name, expectedText) => {
    renderWithProviders(<PublisherCard id="pub-test" name={name} />);

    expect(screen.getByText(expectedText)).toBeInTheDocument();
  });

  it("should display publisher label and link metadata", () => {
    renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    expect(screen.getByText("Publisher")).toBeInTheDocument();
    expect(screen.getByRole("link")).toHaveAttribute(
      "href",
      `/shop?publisher=${mockPublisher.id}`,
    );
    expect(screen.getByText("Explore")).toBeInTheDocument();
  });

  it.each([
    ["hover effects", ".group", ["hover:shadow-lg"]],
    ["transition classes", ".group", ["transition-all", "duration-500"]],
    ["rounded corners", ".rounded-lg", undefined],
    ["aspect-square ratio", ".aspect-square", undefined],
    ["gradient background", ".bg-linear-to-br", undefined],
    ["overlay effect", ".absolute.inset-0", undefined],
  ])("should have %s", (_label, selector, classNames) => {
    const { container } = renderWithProviders(
      <PublisherCard id={mockPublisher.id} name={mockPublisher.name} />,
    );

    const element = container.querySelector(selector);

    if (classNames) {
      expect(element).toHaveClass(...classNames);
      return;
    }

    expect(element).toBeInTheDocument();
  });

  it("should render title with serif font", () => {
    renderWithProviders(
      <PublisherCard id={mockPublisher.id} name="Penguin Random House" />,
    );

    const title = screen.getByText("Penguin Random House");
    expect(title).toHaveClass("font-serif");
  });
});
