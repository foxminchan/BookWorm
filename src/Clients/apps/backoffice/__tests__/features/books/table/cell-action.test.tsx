import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, render, screen, waitFor } from "@testing-library/react";
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

vi.mock("@/components/confirm-dialog", () => ({
  ConfirmDialog: ({
    open,
    onOpenChange,
    title,
    description,
    actionLabel,
    onConfirm,
  }: {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    title: string;
    description: string;
    actionLabel: string;
    onConfirm: () => Promise<void>;
  }) =>
    open ? (
      <div role="alertdialog">
        <p>{title}</p>
        <p>{description}</p>
        <button onClick={() => onOpenChange(false)}>Cancel</button>
        <button onClick={onConfirm}>{actionLabel}</button>
      </div>
    ) : null,
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

    // Wait for dialog to open
    await screen.findByRole("alertdialog");

    await waitFor(() => {
      expect(screen.getByText("Delete Book")).toBeInTheDocument();
      expect(
        screen.getByText(
          new RegExp(`Are you sure you want to delete "${mockBook.name}`),
        ),
      ).toBeInTheDocument();
    });
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

  it("calls delete mutation when confirming deletion", async () => {
    const user = userEvent.setup();
    const mockMutate = vi.fn();
    mockUseDeleteBook.mockReturnValue({
      mutate: mockMutate,
      isPending: false,
    });

    renderWithQueryClient(<CellAction book={mockBook} />);

    // Open delete dialog
    await user.click(screen.getByLabelText(`Delete ${mockBook.name}`));
    await screen.findByRole("alertdialog");

    // Click confirm delete
    await user.click(screen.getByRole("button", { name: "Delete" }));

    expect(mockMutate).toHaveBeenCalledWith(
      mockBook.id,
      expect.objectContaining({
        onSuccess: expect.any(Function),
      }),
    );
  });

  it("closes dialog and shows toast on successful deletion", async () => {
    const user = userEvent.setup();
    const mockMutate = vi.fn();
    mockUseDeleteBook.mockReturnValue({
      mutate: mockMutate,
      isPending: false,
    });

    renderWithQueryClient(<CellAction book={mockBook} />);

    // Open and confirm delete
    await user.click(screen.getByLabelText(`Delete ${mockBook.name}`));
    await screen.findByRole("alertdialog");

    await user.click(screen.getByRole("button", { name: "Delete" }));

    // Invoke the onSuccess callback
    const onSuccess = mockMutate.mock.calls[0]?.[1]?.onSuccess;
    expect(onSuccess).toBeDefined();
    act(() => {
      onSuccess!();
    });

    await waitFor(() => {
      expect(screen.queryByText("Delete Book")).not.toBeInTheDocument();
    });
  });
});
