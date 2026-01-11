import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { StatusFilter } from "@/features/orders/status-filter";

describe("StatusFilter", () => {
  it("renders all status filter buttons", () => {
    const onStatusChange = vi.fn();
    render(
      <StatusFilter statusFilter={undefined} onStatusChange={onStatusChange} />,
    );

    expect(screen.getByText("All Orders")).toBeInTheDocument();
    expect(screen.getByText("New")).toBeInTheDocument();
    expect(screen.getByText("Completed")).toBeInTheDocument();
    expect(screen.getByText("Cancelled")).toBeInTheDocument();
  });

  it("highlights the active filter", () => {
    const onStatusChange = vi.fn();
    render(<StatusFilter statusFilter="New" onStatusChange={onStatusChange} />);

    const newButton = screen.getByText("New");
    const allButton = screen.getByText("All Orders");

    expect(newButton).toHaveClass("bg-primary");
    expect(allButton).not.toHaveClass("bg-primary");
  });

  it("calls onStatusChange when clicking a filter button", async () => {
    const user = userEvent.setup();
    const onStatusChange = vi.fn();
    render(
      <StatusFilter statusFilter={undefined} onStatusChange={onStatusChange} />,
    );

    await user.click(screen.getByText("New"));

    expect(onStatusChange).toHaveBeenCalledWith("New");
    expect(onStatusChange).toHaveBeenCalledTimes(1);
  });

  it("calls onStatusChange with undefined when clicking All Orders", async () => {
    const user = userEvent.setup();
    const onStatusChange = vi.fn();
    render(<StatusFilter statusFilter="New" onStatusChange={onStatusChange} />);

    await user.click(screen.getByText("All Orders"));

    expect(onStatusChange).toHaveBeenCalledWith(undefined);
  });

  it("handles switching between different statuses", async () => {
    const user = userEvent.setup();
    const onStatusChange = vi.fn();
    render(
      <StatusFilter statusFilter={undefined} onStatusChange={onStatusChange} />,
    );

    await user.click(screen.getByText("New"));
    expect(onStatusChange).toHaveBeenCalledWith("New");

    await user.click(screen.getByText("Completed"));
    expect(onStatusChange).toHaveBeenCalledWith("Completed");

    await user.click(screen.getByText("Cancelled"));
    expect(onStatusChange).toHaveBeenCalledWith("Cancelled");
  });

  it("renders buttons with correct styling", () => {
    const onStatusChange = vi.fn();
    const { container } = render(
      <StatusFilter statusFilter={undefined} onStatusChange={onStatusChange} />,
    );

    const buttons = container.querySelectorAll("button");
    expect(buttons).toHaveLength(4);

    buttons.forEach((button) => {
      expect(button).toHaveClass("whitespace-nowrap");
    });
  });

  it("renders all status values correctly", () => {
    const onStatusChange = vi.fn();
    render(
      <StatusFilter statusFilter={undefined} onStatusChange={onStatusChange} />,
    );

    const statuses = ["All Orders", "New", "Completed", "Cancelled"];
    statuses.forEach((status) => {
      expect(screen.getByText(status)).toBeInTheDocument();
    });
  });

  it("has proper button structure", () => {
    const onStatusChange = vi.fn();
    const { container } = render(
      <StatusFilter statusFilter={undefined} onStatusChange={onStatusChange} />,
    );

    const buttonContainer = container.querySelector(
      ".flex.gap-2.overflow-x-auto",
    );
    expect(buttonContainer).toBeInTheDocument();
  });
});
