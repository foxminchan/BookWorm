import { faker } from "@faker-js/faker";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createMockAuthor } from "@/__tests__/factories";
import { AuthorsFilter } from "@/features/books/filters/authors-filter";

vi.mock("@workspace/api-hooks/catalog/authors/useAuthors");

const mockUseAuthors = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/catalog/authors/useAuthors", () => ({
  default: mockUseAuthors,
}));

describe("AuthorsFilter", () => {
  let mockAuthors: ReturnType<typeof createMockAuthor>[];

  beforeEach(() => {
    // Reset faker seed for consistent test data
    faker.seed(123);
    // Recreate mock data for each test
    mockAuthors = [
      createMockAuthor({ name: "John Doe" }),
      createMockAuthor({ name: "Jane Smith" }),
    ];
  });

  it("renders authors label", () => {
    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });

    render(
      <AuthorsFilter
        selectedAuthors={[]}
        onToggle={vi.fn()}
        onClear={vi.fn()}
      />,
    );

    expect(screen.getByText("Authors")).toBeInTheDocument();
  });

  it("renders all author buttons", () => {
    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });

    render(
      <AuthorsFilter
        selectedAuthors={[]}
        onToggle={vi.fn()}
        onClear={vi.fn()}
      />,
    );

    expect(screen.getByText(mockAuthors[0]!.name)).toBeInTheDocument();
    expect(screen.getByText(mockAuthors[1]!.name)).toBeInTheDocument();
  });

  it("highlights selected authors", () => {
    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });

    render(
      <AuthorsFilter
        selectedAuthors={[mockAuthors[0]!.id]}
        onToggle={vi.fn()}
        onClear={vi.fn()}
      />,
    );

    const selectedButton = screen
      .getByText(mockAuthors[0]!.name)
      .closest("button");
    expect(selectedButton).toHaveAttribute("aria-pressed", "true");
  });

  it("calls onToggle when author button clicked", async () => {
    const user = userEvent.setup();
    const onToggle = vi.fn();
    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });

    render(
      <AuthorsFilter
        selectedAuthors={[]}
        onToggle={onToggle}
        onClear={vi.fn()}
      />,
    );

    const button = screen.getByText(mockAuthors[1]!.name);
    await user.click(button);

    expect(onToggle).toHaveBeenCalledWith(mockAuthors[1]!.id);
  });

  it("shows clear button when authors are selected", () => {
    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });

    render(
      <AuthorsFilter
        selectedAuthors={[mockAuthors[0]!.id, mockAuthors[1]!.id]}
        onToggle={vi.fn()}
        onClear={vi.fn()}
      />,
    );

    expect(screen.getByText("Clear authors")).toBeInTheDocument();
  });

  it("does not show clear button when no authors selected", () => {
    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });

    render(
      <AuthorsFilter
        selectedAuthors={[]}
        onToggle={vi.fn()}
        onClear={vi.fn()}
      />,
    );

    expect(screen.queryByText("Clear authors")).not.toBeInTheDocument();
  });

  it("calls onClear when clear button clicked", async () => {
    const user = userEvent.setup();
    const onClear = vi.fn();
    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });

    render(
      <AuthorsFilter
        selectedAuthors={[mockAuthors[0]!.id]}
        onToggle={vi.fn()}
        onClear={onClear}
      />,
    );

    const clearButton = screen.getByText("Clear authors");
    await user.click(clearButton);

    expect(onClear).toHaveBeenCalled();
  });

  it("has proper accessibility attributes", () => {
    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });

    render(
      <AuthorsFilter
        selectedAuthors={[mockAuthors[0]!.id]}
        onToggle={vi.fn()}
        onClear={vi.fn()}
      />,
    );

    const group = screen.getByRole("group", { name: "Filter by authors" });
    expect(group).toBeInTheDocument();

    const clearButton = screen.getByLabelText("Clear all author filters");
    expect(clearButton).toBeInTheDocument();
  });

  it("handles undefined authors data", () => {
    mockUseAuthors.mockReturnValue({
      data: undefined,
      isLoading: false,
    });

    render(
      <AuthorsFilter
        selectedAuthors={[]}
        onToggle={vi.fn()}
        onClear={vi.fn()}
      />,
    );

    expect(screen.getByText("Authors")).toBeInTheDocument();
  });

  it("calls onClear when clear button clicked", async () => {
    const user = userEvent.setup();
    const onClear = vi.fn();
    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });

    render(
      <AuthorsFilter
        selectedAuthors={["author-1"]}
        onToggle={vi.fn()}
        onClear={onClear}
      />,
    );

    const clearButton = screen.getByRole("button", { name: /clear/i });
    await user.click(clearButton);

    expect(onClear).toHaveBeenCalled();
  });

  it("renders multiple selected authors", () => {
    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });

    render(
      <AuthorsFilter
        selectedAuthors={[mockAuthors[0]!.id, mockAuthors[1]!.id]}
        onToggle={vi.fn()}
        onClear={vi.fn()}
      />,
    );

    const selectedButtons = screen.getAllByRole("button", {
      pressed: true,
    });
    expect(selectedButtons.length).toBeGreaterThanOrEqual(2);
  });
});
