import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import PublishersGrid from "@/features/catalog/publisher/publishers-grid";

import { renderWithProviders } from "../../utils/test-utils";

type Publisher = {
  id: string;
  name: string | null;
};

const mockPublishers: Publisher[] = [
  { id: faker.string.uuid(), name: faker.company.name() },
  { id: faker.string.uuid(), name: faker.company.name() },
  { id: faker.string.uuid(), name: faker.company.name() },
];

describe("PublishersGrid", () => {
  it("should render all publishers", () => {
    const publishers = [
      { ...mockPublishers[0]!, name: "Penguin Random House" },
      { ...mockPublishers[1]!, name: "HarperCollins" },
      { ...mockPublishers[2]!, name: "Simon & Schuster" },
    ];

    renderWithProviders(
      <PublishersGrid publishers={publishers} isLoading={false} />,
    );

    expect(screen.getByText("Penguin Random House")).toBeInTheDocument();
    expect(screen.getByText("HarperCollins")).toBeInTheDocument();
    expect(screen.getByText("Simon & Schuster")).toBeInTheDocument();
  });

  it("should render loading skeletons when loading", () => {
    const { container } = renderWithProviders(
      <PublishersGrid publishers={[]} isLoading={true} />,
    );

    // Should render 9 skeleton cards
    const skeletons = container.querySelectorAll(".animate-pulse");
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("should not render publishers when loading", () => {
    renderWithProviders(
      <PublishersGrid publishers={mockPublishers} isLoading={true} />,
    );

    expect(screen.queryByText("Penguin Random House")).not.toBeInTheDocument();
  });

  it("should render empty grid when no publishers", () => {
    const { container } = renderWithProviders(
      <PublishersGrid publishers={[]} isLoading={false} />,
    );

    const grid = container.querySelector(".grid");
    expect(grid).toBeInTheDocument();
    expect(grid?.children).toHaveLength(0);
  });

  it("should display 'Unknown Publisher' for null names", () => {
    const publishersWithNull: Publisher[] = [
      { ...mockPublishers[0]!, name: null },
      { ...mockPublishers[1]!, name: "Penguin Random House" },
    ];

    renderWithProviders(
      <PublishersGrid publishers={publishersWithNull} isLoading={false} />,
    );

    expect(screen.getByText("Unknown Publisher")).toBeInTheDocument();
    expect(screen.getByText("Penguin Random House")).toBeInTheDocument();
  });

  it("should render grid with proper styling", () => {
    const { container } = renderWithProviders(
      <PublishersGrid publishers={mockPublishers} isLoading={false} />,
    );

    const grid = container.querySelector(".grid");
    expect(grid).toHaveClass("grid-cols-2");
  });

  it("should have responsive grid layout", () => {
    const { container } = renderWithProviders(
      <PublishersGrid publishers={mockPublishers} isLoading={false} />,
    );

    const grid = container.querySelector(".grid");
    expect(grid).toHaveClass("md:grid-cols-3");
  });

  it("should render links for all publishers", () => {
    renderWithProviders(
      <PublishersGrid publishers={mockPublishers} isLoading={false} />,
    );

    const links = screen.getAllByRole("link");
    expect(links).toHaveLength(3);
  });

  it("should create proper publisher filter links", () => {
    const publishers = [{ ...mockPublishers[0]!, id: "pub-1" }];
    renderWithProviders(
      <PublishersGrid publishers={publishers} isLoading={false} />,
    );

    const link = screen.getByRole("link");
    expect(link).toHaveAttribute("href", "/shop?publisher=pub-1");
  });

  it("should handle single publisher", () => {
    const publishers = [
      { ...mockPublishers[0]!, name: "Penguin Random House" },
    ];
    renderWithProviders(
      <PublishersGrid publishers={publishers} isLoading={false} />,
    );

    expect(screen.getByText("Penguin Random House")).toBeInTheDocument();
    const links = screen.getAllByRole("link");
    expect(links).toHaveLength(1);
  });

  it("should have gap between items", () => {
    const { container } = renderWithProviders(
      <PublishersGrid publishers={mockPublishers} isLoading={false} />,
    );

    const grid = container.querySelector(".gap-6");
    expect(grid).toBeInTheDocument();
  });

  it("should have bottom margin", () => {
    const { container } = renderWithProviders(
      <PublishersGrid publishers={mockPublishers} isLoading={false} />,
    );

    const grid = container.querySelector(".mb-20");
    expect(grid).toBeInTheDocument();
  });

  it("should render multiple publishers correctly", () => {
    const manyPublishers: Publisher[] = Array.from({ length: 10 }, (_, i) => ({
      id: `pub-${i}`,
      name: `Publisher ${i + 1}`,
    }));

    renderWithProviders(
      <PublishersGrid publishers={manyPublishers} isLoading={false} />,
    );

    const links = screen.getAllByRole("link");
    expect(links).toHaveLength(10);
  });
});
