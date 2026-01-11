import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { createMockFeedback } from "@/__tests__/factories";
import { CellAction } from "@/features/reviews/table/cell-action";

vi.mock("@workspace/api-hooks/rating/useDeleteFeedback");

const mockUseDeleteFeedback = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/rating/useDeleteFeedback", () => ({
  default: mockUseDeleteFeedback,
}));

describe("Reviews CellAction", () => {
  const mockReview = createMockFeedback({ rating: 4.5 });

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

  it("renders delete button", () => {
    mockUseDeleteFeedback.mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    });

    const { container } = renderWithQueryClient(
      <CellAction feedback={mockReview} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    expect(deleteButton).toBeInTheDocument();
  });

  it("opens delete dialog when delete button clicked", async () => {
    const user = userEvent.setup();
    mockUseDeleteFeedback.mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    });

    const { container } = renderWithQueryClient(
      <CellAction feedback={mockReview} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    await user.click(deleteButton!);

    await waitFor(() => {
      expect(screen.getByText("Delete Review")).toBeInTheDocument();
      expect(
        screen.getByText(
          new RegExp(`The customer gave it a ${mockReview.rating} star rating`),
        ),
      ).toBeInTheDocument();
    });
  });

  it("disables delete button when mutation is pending", () => {
    mockUseDeleteFeedback.mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: true,
    });

    const { container } = renderWithQueryClient(
      <CellAction feedback={mockReview} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    expect(deleteButton).toBeDisabled();
  });

  it("shows rating in delete confirmation dialog", async () => {
    const user = userEvent.setup();
    mockUseDeleteFeedback.mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    });

    const { container } = renderWithQueryClient(
      <CellAction feedback={mockReview} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    await user.click(deleteButton!);

    await waitFor(() => {
      expect(
        screen.getByText(
          new RegExp(`The customer gave it a ${mockReview.rating} star rating`),
        ),
      ).toBeInTheDocument();
    });
  });

  it("handles different rating values in confirmation", async () => {
    const user = userEvent.setup();
    mockUseDeleteFeedback.mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    });

    const lowRatingReview = { ...mockReview, rating: 1 };
    const { container } = renderWithQueryClient(
      <CellAction feedback={lowRatingReview} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    await user.click(deleteButton!);

    await waitFor(() => {
      expect(
        screen.getByText(/The customer gave it a 1 star rating/),
      ).toBeInTheDocument();
    });
  });

  it("calls delete mutation when confirmed", async () => {
    const user = userEvent.setup();
    const mutateAsyncFn = vi.fn().mockResolvedValue(undefined);
    mockUseDeleteFeedback.mockReturnValue({
      mutateAsync: mutateAsyncFn,
      isPending: false,
    });

    const { container } = renderWithQueryClient(
      <CellAction feedback={mockReview} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    await user.click(deleteButton!);

    await waitFor(() => {
      expect(screen.getByText("Delete Review")).toBeInTheDocument();
    });

    const confirmButton = screen.getByRole("button", { name: /delete/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(mutateAsyncFn).toHaveBeenCalledWith(
        mockReview.id,
        expect.any(Object),
      );
    });
  });

  it("closes delete dialog on successful mutation", async () => {
    const user = userEvent.setup();
    const mutateAsyncFn = vi
      .fn()
      .mockImplementation((feedbackId, { onSuccess }) => {
        onSuccess?.();
        return Promise.resolve();
      });
    mockUseDeleteFeedback.mockReturnValue({
      mutateAsync: mutateAsyncFn,
      isPending: false,
    });

    const { container } = renderWithQueryClient(
      <CellAction feedback={mockReview} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    await user.click(deleteButton!);

    const confirmButton = screen.getByRole("button", { name: /delete/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(mutateAsyncFn).toHaveBeenCalled();
    });
  });
});
