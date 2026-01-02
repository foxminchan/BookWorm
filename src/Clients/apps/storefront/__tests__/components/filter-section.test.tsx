import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { FilterSection } from "@/components/filter-section";

import { renderWithProviders } from "../utils/test-utils";

const mockItems = [
  { id: "1", name: "Fiction" },
  { id: "2", name: "Science Fiction" },
  { id: "3", name: "Mystery" },
  { id: "4", name: "Romance" },
  { id: "5", name: "Thriller" },
  { id: "6", name: "Fantasy" },
  { id: "7", name: "Biography" },
];

describe("FilterSection", () => {
  it("should render title and items", () => {
    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems}
        selectedItems={[]}
        onToggle={vi.fn()}
      />,
    );

    expect(screen.getByText("Categories")).toBeInTheDocument();
    expect(screen.getByText("Fiction")).toBeInTheDocument();
    expect(screen.getByText("Science Fiction")).toBeInTheDocument();
  });

  it("should show only maxVisibleItems by default", () => {
    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems}
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

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems}
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

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems}
        selectedItems={["1"]}
        onToggle={mockToggle}
      />,
    );

    const fictionCheckbox = screen.getByRole("checkbox", { name: "Fiction" });
    await user.click(fictionCheckbox);

    expect(mockToggle).toHaveBeenCalledWith("1");
  });

  it("should show checked state for selected items", () => {
    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems}
        selectedItems={["1", "3"]}
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

    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems}
        selectedItems={[]}
        onToggle={vi.fn()}
        searchable
      />,
    );

    // Open search
    const searchButton = screen.getByRole("button", {
      name: /search categories/i,
    });
    await user.click(searchButton);

    // Type search term
    const searchInput = screen.getByPlaceholderText(/search categories/i);
    await user.type(searchInput, "fiction");

    // Should show matching items
    expect(screen.getByText("Fiction")).toBeInTheDocument();
    expect(screen.getByText("Science Fiction")).toBeInTheDocument();
    // Should not show non-matching items
    expect(screen.queryByText("Biography")).not.toBeInTheDocument();
  });

  it("should be case-insensitive when searching", async () => {
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
    await user.type(searchInput, "FICTION");

    expect(screen.getByText("Fiction")).toBeInTheDocument();
    expect(screen.getByText("Science Fiction")).toBeInTheDocument();
  });

  it("should not show toggle button when items count is less than maxVisibleItems", () => {
    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems.slice(0, 3)}
        selectedItems={[]}
        onToggle={vi.fn()}
        maxVisibleItems={5}
      />,
    );

    const showMoreButton = screen.queryByRole("button", { name: /show more/i });
    expect(showMoreButton).not.toBeInTheDocument();
  });

  it("should not collapse when collapsible is false", () => {
    renderWithProviders(
      <FilterSection
        title="Categories"
        items={mockItems}
        selectedItems={[]}
        onToggle={vi.fn()}
        collapsible={false}
      />,
    );

    // All items should be visible
    expect(screen.getByText("Biography")).toBeInTheDocument();
    // Toggle button should not exist
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

    await user.type(searchInput, "rom");
    expect(screen.getByText("Romance")).toBeInTheDocument();
    expect(screen.queryByText("Fiction")).not.toBeInTheDocument();

    await user.clear(searchInput);
    await user.type(searchInput, "mystery");
    expect(screen.getByText("Mystery")).toBeInTheDocument();
    expect(screen.queryByText("Romance")).not.toBeInTheDocument();
  });
});
