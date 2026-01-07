import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { Pagination } from "@/components/pagination";

import { renderWithProviders } from "../utils/test-utils";

describe("Pagination", () => {
  it("should not render when totalPages is 1 or less", () => {
    const { container } = renderWithProviders(
      <Pagination currentPage={1} totalPages={1} onPageChange={vi.fn()} />,
    );

    expect(container.firstChild).toBeNull();
  });

  it("should render all page numbers when totalPages is 5 or less", () => {
    renderWithProviders(
      <Pagination currentPage={1} totalPages={5} onPageChange={vi.fn()} />,
    );

    expect(screen.getByText("1")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
    expect(screen.getByText("4")).toBeInTheDocument();
    expect(screen.getByText("5")).toBeInTheDocument();
  });

  it("should show first 5 pages when current page is near beginning", () => {
    renderWithProviders(
      <Pagination currentPage={2} totalPages={10} onPageChange={vi.fn()} />,
    );

    expect(screen.getByText("1")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
    expect(screen.getByText("4")).toBeInTheDocument();
    expect(screen.getByText("5")).toBeInTheDocument();
    expect(screen.queryByText("6")).not.toBeInTheDocument();
  });

  it("should show last 5 pages when current page is near end", () => {
    renderWithProviders(
      <Pagination currentPage={9} totalPages={10} onPageChange={vi.fn()} />,
    );

    expect(screen.getByText("6")).toBeInTheDocument();
    expect(screen.getByText("7")).toBeInTheDocument();
    expect(screen.getByText("8")).toBeInTheDocument();
    expect(screen.getByText("9")).toBeInTheDocument();
    expect(screen.getByText("10")).toBeInTheDocument();
    expect(screen.queryByText("5")).not.toBeInTheDocument();
  });

  it("should show 5 pages centered around current page in the middle", () => {
    renderWithProviders(
      <Pagination currentPage={5} totalPages={10} onPageChange={vi.fn()} />,
    );

    expect(screen.getByText("3")).toBeInTheDocument();
    expect(screen.getByText("4")).toBeInTheDocument();
    expect(screen.getByText("5")).toBeInTheDocument();
    expect(screen.getByText("6")).toBeInTheDocument();
    expect(screen.getByText("7")).toBeInTheDocument();
  });

  it("should call onPageChange when a page number is clicked", async () => {
    const user = userEvent.setup();
    const handlePageChange = vi.fn();

    renderWithProviders(
      <Pagination
        currentPage={1}
        totalPages={5}
        onPageChange={handlePageChange}
      />,
    );

    await user.click(screen.getByText("3"));

    expect(handlePageChange).toHaveBeenCalledWith(3);
  });

  it("should call onPageChange when next button is clicked", async () => {
    const user = userEvent.setup();
    const handlePageChange = vi.fn();

    renderWithProviders(
      <Pagination
        currentPage={2}
        totalPages={5}
        onPageChange={handlePageChange}
      />,
    );

    const nextButton = screen.getByRole("button", { name: /next/i });
    await user.click(nextButton);

    expect(handlePageChange).toHaveBeenCalledWith(3);
  });

  it("should call onPageChange when previous button is clicked", async () => {
    const user = userEvent.setup();
    const handlePageChange = vi.fn();

    renderWithProviders(
      <Pagination
        currentPage={3}
        totalPages={5}
        onPageChange={handlePageChange}
      />,
    );

    const prevButton = screen.getByRole("button", { name: /previous/i });
    await user.click(prevButton);

    expect(handlePageChange).toHaveBeenCalledWith(2);
  });

  it("should disable previous button on first page", () => {
    renderWithProviders(
      <Pagination currentPage={1} totalPages={5} onPageChange={vi.fn()} />,
    );

    const prevButton = screen.getByRole("button", { name: /previous/i });
    expect(prevButton).toBeDisabled();
  });

  it("should disable next button on last page", () => {
    renderWithProviders(
      <Pagination currentPage={5} totalPages={5} onPageChange={vi.fn()} />,
    );

    const nextButton = screen.getByRole("button", { name: /next/i });
    expect(nextButton).toBeDisabled();
  });

  it("should highlight current page", () => {
    renderWithProviders(
      <Pagination currentPage={3} totalPages={5} onPageChange={vi.fn()} />,
    );

    const currentPageButton = screen.getByRole("button", {
      name: /current page, page 3/i,
    });
    expect(currentPageButton).toHaveClass("bg-primary");
    expect(currentPageButton).toHaveAttribute("aria-current", "page");
  });

  it("should call onPageChange when first page button is clicked", async () => {
    const user = userEvent.setup();
    const handlePageChange = vi.fn();

    renderWithProviders(
      <Pagination
        currentPage={5}
        totalPages={10}
        onPageChange={handlePageChange}
      />,
    );

    const firstButton = screen.getByRole("button", { name: /first/i });
    await user.click(firstButton);

    expect(handlePageChange).toHaveBeenCalledWith(1);
  });

  it("should call onPageChange when last page button is clicked", async () => {
    const user = userEvent.setup();
    const handlePageChange = vi.fn();

    renderWithProviders(
      <Pagination
        currentPage={1}
        totalPages={10}
        onPageChange={handlePageChange}
      />,
    );

    const lastButton = screen.getByRole("button", { name: /last/i });
    await user.click(lastButton);

    expect(handlePageChange).toHaveBeenCalledWith(10);
  });

  it("should disable first page button when on first page", () => {
    renderWithProviders(
      <Pagination currentPage={1} totalPages={10} onPageChange={vi.fn()} />,
    );

    const firstButton = screen.getByRole("button", { name: /first/i });
    expect(firstButton).toBeDisabled();
  });

  it("should disable last page button when on last page", () => {
    renderWithProviders(
      <Pagination currentPage={10} totalPages={10} onPageChange={vi.fn()} />,
    );

    const lastButton = screen.getByRole("button", { name: /last/i });
    expect(lastButton).toBeDisabled();
  });
});
