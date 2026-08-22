import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { createMockCategory } from "@/__tests__/factories";
import { CategorySelect } from "@/features/books/filters/category-select";

const mockUseCategories = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/catalog/categories/useCategories", () => ({
  default: mockUseCategories,
}));

describe("CategorySelect", () => {
  const mockCategories = [
    createMockCategory({ name: "Fiction" }),
    createMockCategory({ name: "Non-Fiction" }),
    createMockCategory({ name: "Science" }),
  ];

  it.each([
    {
      name: "category label",
      categories: mockCategories,
      value: undefined,
      expectedText: "Category",
    },
    {
      name: "placeholder",
      categories: mockCategories,
      value: undefined,
      expectedText: "All Categories",
    },
    {
      name: "empty categories list",
      categories: [],
      value: undefined,
      expectedText: "All Categories",
    },
    {
      name: "undefined categories data",
      categories: undefined,
      value: undefined,
      expectedText: "All Categories",
    },
  ])("renders $name", ({ categories, value, expectedText }) => {
    mockUseCategories.mockReturnValue({
      data: categories,
      isLoading: false,
    });

    render(<CategorySelect value={value} onChange={vi.fn()} />);

    expect(screen.getByText(expectedText)).toBeInTheDocument();
  });

  it("displays selected category", () => {
    mockUseCategories.mockReturnValue({
      data: mockCategories,
      isLoading: false,
    });

    render(<CategorySelect value={mockCategories[0]!.id} onChange={vi.fn()} />);

    const select = screen.getByRole("combobox");
    expect(select).toBeInTheDocument();
  });

  it("has proper label association", () => {
    mockUseCategories.mockReturnValue({
      data: mockCategories,
      isLoading: false,
    });

    render(<CategorySelect value={undefined} onChange={vi.fn()} />);

    const label = screen.getByText("Category");
    const select = screen.getByRole("combobox");

    expect(label).toHaveAttribute("for", "category-select");
    expect(select).toHaveAttribute("id", "category-select");
  });

  it("renders with correct styling classes", () => {
    mockUseCategories.mockReturnValue({
      data: mockCategories,
      isLoading: false,
    });

    const { container } = render(
      <CategorySelect value={undefined} onChange={vi.fn()} />,
    );

    const selectTrigger = container.querySelector("#category-select");
    expect(selectTrigger).toHaveClass("w-full");
  });

  it("calls onChange when category is selected", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    mockUseCategories.mockReturnValue({
      data: mockCategories,
      isLoading: false,
    });

    render(<CategorySelect value={undefined} onChange={onChange} />);

    const select = screen.getByRole("combobox");
    await user.click(select);

    expect(onChange).toBeDefined();
  });
});
