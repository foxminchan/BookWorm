import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { AuthorsFilter } from "@/features/books/filters/authors-filter";

const mockUseAuthors = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/catalog/authors/useAuthors", () => ({
  default: mockUseAuthors,
}));

describe("AuthorsFilter", () => {
  // Use static mock data to avoid faker seed issues
  const mockAuthors = [
    { id: "author-1", name: "John Doe" },
    { id: "author-2", name: "Jane Smith" },
  ];

  beforeEach(() => {
    // Clear all mocks before each test
    vi.clearAllMocks();
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

    // Check that both author buttons are rendered
    const buttons = screen.getAllByRole("button");
    const authorButtons = buttons.filter((btn) =>
      btn.getAttribute("aria-label")?.includes("filter"),
    );
    expect(authorButtons.length).toBe(2);
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
});
