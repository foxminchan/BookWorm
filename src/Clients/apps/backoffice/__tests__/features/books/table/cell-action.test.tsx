import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createMockBook } from "@/__tests__/factories";
import { CellAction } from "@/features/books/table/cell-action";

const mockUseDeleteBook = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/catalog/books/useDeleteBook", () => ({
  default: mockUseDeleteBook,
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: vi.fn(),
    refresh: vi.fn(),
  }),
}));

describe("Books CellAction", () => {
  const mockBook = createMockBook({ status: "InStock" });

  const renderWithQueryClient = (component: React.ReactElement) => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    return render(
      <QueryClientProvider client={queryClient}>
        {component}
      </QueryClientProvider>,
    );
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders edit and delete buttons", () => {
    mockUseDeleteBook.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });

    renderWithQueryClient(<CellAction book={mockBook} />);

    expect(screen.getByLabelText(`Edit ${mockBook.name}`)).toBeInTheDocument();
    expect(
      screen.getByLabelText(`Delete ${mockBook.name}`),
    ).toBeInTheDocument();
  });

  it("edit button links to book edit page", () => {
    mockUseDeleteBook.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });

    renderWithQueryClient(<CellAction book={mockBook} />);

    const editLink = screen.getByLabelText(`Edit ${mockBook.name}`);
    expect(editLink).toHaveAttribute("href", `/books/${mockBook.id}`);
  });

  it("opens delete dialog when delete button clicked", async () => {
    const user = userEvent.setup();
    mockUseDeleteBook.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });

    renderWithQueryClient(<CellAction book={mockBook} />);

    const deleteButton = screen.getByLabelText(`Delete ${mockBook.name}`);
    await user.click(deleteButton);

    await waitFor(() => {
      expect(screen.getByText("Delete Book")).toBeInTheDocument();
      expect(
        screen.getByText(
          new RegExp(`Are you sure you want to delete "${mockBook.name}`),
        ),
      ).toBeInTheDocument();
    });
  });

  it("disables delete button when mutation is pending", () => {
    mockUseDeleteBook.mockReturnValue({
      mutate: vi.fn(),
      isPending: true,
    });

    renderWithQueryClient(<CellAction book={mockBook} />);

    const deleteButton = screen.getByLabelText(`Delete ${mockBook.name}`);
    expect(deleteButton).toBeDisabled();
  });

  it("shows loading spinner when mutation is pending", () => {
    mockUseDeleteBook.mockReturnValue({
      mutate: vi.fn(),
      isPending: true,
    });

    const { container } = renderWithQueryClient(<CellAction book={mockBook} />);

    expect(container.querySelector(".animate-spin")).toBeInTheDocument();
  });

  it("handles book with null name", () => {
    mockUseDeleteBook.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });

    const bookWithoutName = { ...mockBook, name: null as any };
    renderWithQueryClient(<CellAction book={bookWithoutName} />);

    expect(screen.getByLabelText("Edit book")).toBeInTheDocument();
    expect(screen.getByLabelText("Delete book")).toBeInTheDocument();
  });

  it("calls delete mutation when confirmed", async () => {
    const user = userEvent.setup();
    const mutateFn = vi.fn();
    mockUseDeleteBook.mockReturnValue({
      mutate: mutateFn,
      isPending: false,
    });

    renderWithQueryClient(<CellAction book={mockBook} />);

    // Click delete button
    const deleteButton = screen.getByLabelText(`Delete ${mockBook.name}`);
    await user.click(deleteButton);

    // Confirm deletion
    await waitFor(() => {
      expect(screen.getByText("Delete Book")).toBeInTheDocument();
    });

    const confirmButton = screen.getByRole("button", { name: /delete/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(mutateFn).toHaveBeenCalledWith(mockBook.id, expect.any(Object));
    });
  });

  it("closes dialog when cancel is clicked", async () => {
    const user = userEvent.setup();
    mockUseDeleteBook.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });

    renderWithQueryClient(<CellAction book={mockBook} />);

    // Click delete button
    const deleteButton = screen.getByLabelText(`Delete ${mockBook.name}`);
    await user.click(deleteButton);

    // Cancel deletion
    await waitFor(() => {
      expect(screen.getByText("Delete Book")).toBeInTheDocument();
    });

    const cancelButton = screen.getByRole("button", { name: /cancel/i });
    await user.click(cancelButton);

    await waitFor(() => {
      expect(screen.queryByText("Delete Book")).not.toBeInTheDocument();
    });
  });

  it("closes delete dialog on successful mutation", async () => {
    const user = userEvent.setup();
    const mutateFn = vi.fn((bookId, { onSuccess }) => {
      onSuccess?.();
    });
    mockUseDeleteBook.mockReturnValue({
      mutate: mutateFn,
      isPending: false,
    });

    renderWithQueryClient(<CellAction book={mockBook} />);

    const deleteButton = screen.getByLabelText(`Delete ${mockBook.name}`);
    await user.click(deleteButton);

    const confirmButton = screen.getByRole("button", { name: /delete/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(mutateFn).toHaveBeenCalled();
    });
  });
});
