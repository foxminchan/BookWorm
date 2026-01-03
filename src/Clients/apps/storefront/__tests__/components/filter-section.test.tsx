import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { FilterSection } from "@/components/filter-section";

import { renderWithProviders } from "../utils/test-utils";

const mockItems = Array.from({ length: 7 }, () => ({
  id: faker.string.uuid(),
  name: faker.commerce.department(),
}));

describe("FilterSection", () => {
  it("should render title and items", () => {
    const items = [
      { id: faker.string.uuid(), name: "Fiction" },
      { id: faker.string.uuid(), name: "Science Fiction" },
    ];

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={items}
        selectedItems={[]}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.getByText("Categories")).toBeInTheDocument();
    expect(screen.getByText("Fiction")).toBeInTheDocument();
    expect(screen.getByText("Science Fiction")).toBeInTheDocument();
  });

  it("should show only maxVisibleItems by default", () => {
    const items = [
      { id: faker.string.uuid(), name: "Fiction" },
      { id: faker.string.uuid(), name: "Science Fiction" },
      { id: faker.string.uuid(), name: "Mystery" },
      { id: faker.string.uuid(), name: "Romance" },
    ];

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={items}
        selectedItems={[]}
        onToggle={vi.fn()}
        maxVisibleItems={3}
      />,
    );

    expect(screen.getByText("Fiction")).toBeInTheDocument();
    expect(screen.getByText("Science Fiction")).toBeInTheDocument();
    expect(screen.getByText("Mystery")).toBeInTheDocument();
    expect(screen.queryByText("Romance")).not.toBeInTheDocument();
  });

  it("should expand to show all items when show more is clicked", async () => {
    const user = userEvent.setup();
    const items = Array.from({ length: 7 }, (_, i) => ({
      id: faker.string.uuid(),
      name: i === 6 ? "Biography" : faker.commerce.department(),
    }));

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={items}
        selectedItems={[]}
        onToggle={vi.fn()}
        maxVisibleItems={3}
      />,
    );

    expect(screen.queryByText("Biography")).not.toBeInTheDocument();

    const showMoreButton = screen.getByRole("button", { name: /show more/i });
    await user.click(showMoreButton);

    expect(screen.getByText("Biography")).toBeInTheDocument();
  });

  it("should toggle selected items", async () => {
    const user = userEvent.setup();
    const mockToggle = vi.fn();
    const items = [
      { id: "item-1", name: "Fiction" },
      { id: faker.string.uuid(), name: faker.commerce.department() },
    ];

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={items}
        selectedItems={["item-1"]}
        onToggle={mockToggle}
      />,
    );

    const fictionCheckbox = screen.getByRole("checkbox", { name: "Fiction" });
    await user.click(fictionCheckbox);

    expect(mockToggle).toHaveBeenCalledWith("item-1");
  });

  it("should show checked state for selected items", () => {
    const items = [
      { id: "item-1", name: "Fiction" },
      { id: "item-2", name: "Mystery" },
      { id: "item-3", name: "Romance" },
    ];

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={items}
        selectedItems={["item-1", "item-2"]}
        onToggle={vi.fn()}
      />,
    );

    const fictionCheckbox = screen.getByRole("checkbox", { name: "Fiction" });
    const mysteryCheckbox = screen.getByRole("checkbox", { name: "Mystery" });
    const romanceCheckbox = screen.getByRole("checkbox", { name: "Romance" });

    expect(fictionCheckbox).toBeChecked();
    expect(mysteryCheckbox).toBeChecked();
    expect(romanceCheckbox).not.toBeChecked();
  });

  it("should show search button when searchable is true", () => {
    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems}
        selectedItems={[]}
        onToggle={vi.fn()}
        searchable
      />,
    );

    const searchButton = screen.getByRole("button", {
      name: /search categories/i,
    });
    expect(searchButton).toBeInTheDocument();
  });

  it("should not show search button when searchable is false", () => {
    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems}
        selectedItems={[]}
        onToggle={vi.fn()}
        searchable={false}
      />,
    );

    const searchButton = screen.queryByRole("button", {
      name: /search categories/i,
    });
    expect(searchButton).not.toBeInTheDocument();
  });

  it("should open search input when search button is clicked", async () => {
    const user = userEvent.setup();

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems}
        selectedItems={[]}
        onToggle={vi.fn()}
        searchable
      />,
    );

    const searchButton = screen.getByRole("button", {
      name: /search categories/i,
    });
    await user.click(searchButton);

    const searchInput = screen.getByPlaceholderText(/search categories/i);
    expect(searchInput).toBeInTheDocument();
  });

  it("should filter items based on search term", async () => {
    const user = userEvent.setup();
    const items = [
      { id: faker.string.uuid(), name: "Fiction" },
      { id: faker.string.uuid(), name: "Science Fiction" },
      { id: faker.string.uuid(), name: "Biography" },
      { id: faker.string.uuid(), name: "Mystery" },
    ];

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={items}
        selectedItems={[]}
        onToggle={vi.fn()}
        searchable
      />,
    );

    const searchButton = screen.getByRole("button", {
      name: /search categories/i,
    });
    await user.click(searchButton);

    const searchInput = screen.getByPlaceholderText(/search categories/i);
    await user.type(searchInput, "fiction");

    expect(screen.getByText("Fiction")).toBeInTheDocument();
    expect(screen.getByText("Science Fiction")).toBeInTheDocument();
    expect(screen.queryByText("Biography")).not.toBeInTheDocument();
  });

  it("should be case-insensitive when searching", async () => {
    const user = userEvent.setup();
    const items = [
      { id: faker.string.uuid(), name: "Fiction" },
      { id: faker.string.uuid(), name: "Science Fiction" },
      { id: faker.string.uuid(), name: "Mystery" },
    ];

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={items}
        selectedItems={[]}
        onToggle={vi.fn()}
        searchable
      />,
    );

    const searchButton = screen.getByRole("button", {
      name: /search categories/i,
    });
    await user.click(searchButton);

    const searchInput = screen.getByPlaceholderText(/search categories/i);
    await user.type(searchInput, "FICTION");

    expect(screen.getByText("Fiction")).toBeInTheDocument();
    expect(screen.getByText("Science Fiction")).toBeInTheDocument();
  });

  it("should not show toggle button when items count is less than maxVisibleItems", () => {
    const items = Array.from({ length: 3 }, () => ({
      id: faker.string.uuid(),
      name: faker.commerce.department(),
    }));

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={items}
        selectedItems={[]}
        onToggle={vi.fn()}
        maxVisibleItems={5}
      />,
    );

    const showMoreButton = screen.queryByRole("button", { name: /show more/i });
    expect(showMoreButton).not.toBeInTheDocument();
  });

  it("should not collapse when collapsible is false", () => {
    const items = Array.from({ length: 7 }, (_, i) => ({
      id: faker.string.uuid(),
      name: i === 6 ? "Biography" : faker.commerce.department(),
    }));

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={items}
        selectedItems={[]}
        onToggle={vi.fn()}
        collapsible={false}
      />,
    );

    expect(screen.getByText("Biography")).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: /show more/i }),
    ).not.toBeInTheDocument();
  });

  it("should handle empty items array", () => {
    renderWithProviders(
      <FilterSection
        title="Categories"
        items={[]}
        selectedItems={[]}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.getByText("Categories")).toBeInTheDocument();
    expect(screen.queryByRole("checkbox")).not.toBeInTheDocument();
  });

  it("should focus search input when opened", async () => {
    const user = userEvent.setup();

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems}
        selectedItems={[]}
        onToggle={vi.fn()}
        searchable
      />,
    );

    const searchButton = screen.getByRole("button", {
      name: /search categories/i,
    });
    await user.click(searchButton);

    const searchInput = screen.getByPlaceholderText(/search categories/i);
    expect(searchInput).toHaveFocus();
  });

  it("should update displayed items when search term changes", async () => {
    const user = userEvent.setup();
    const items = [
      { id: faker.string.uuid(), name: "Romance" },
      { id: faker.string.uuid(), name: "Fiction" },
      { id: faker.string.uuid(), name: "Mystery" },
    ];

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={items}
        selectedItems={[]}
        onToggle={vi.fn()}
        searchable
      />,
    );

    const searchButton = screen.getByRole("button", {
      name: /search categories/i,
    });
    await user.click(searchButton);

    const searchInput = screen.getByPlaceholderText(/search categories/i);

    await user.type(searchInput, "rom");
    expect(screen.getByText("Romance")).toBeInTheDocument();
    expect(screen.queryByText("Fiction")).not.toBeInTheDocument();

    await user.clear(searchInput);
    await user.type(searchInput, "mystery");
    expect(screen.getByText("Mystery")).toBeInTheDocument();
    expect(screen.queryByText("Romance")).not.toBeInTheDocument();
  });
});
