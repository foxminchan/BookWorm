import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import type { Order } from "@workspace/types/ordering/orders";

import { createMockOrder } from "@/__tests__/factories";
import { columns } from "@/features/orders/table/columns";

describe("Orders Table Columns", () => {
  const mockOrder = createMockOrder({ status: "New" });

  it("has correct column count", () => {
    expect(columns).toHaveLength(5); // id, date, total, status, actions
  });

  it("renders order ID truncated with # prefix", () => {
    const idColumn = columns[0]!;
    const cell = idColumn.cell as any;
    render(cell({ row: { original: mockOrder } } as any));

    const truncatedId = `#${mockOrder.id.substring(0, 8)}`;
    expect(screen.getByText(truncatedId)).toBeInTheDocument();
  });

  it("renders date in formatted format", () => {
    const dateColumn = columns[1]!;
    const cell = dateColumn.cell as any;
    render(cell({ row: { original: mockOrder } } as any));

    const date = new Date(mockOrder.date);
    const formattedDate = date.toLocaleDateString("en-US", {
      month: "short",
      day: "2-digit",
      year: "numeric",
    });
    expect(screen.getByText(formattedDate)).toBeInTheDocument();
  });

  it("renders total with currency formatting", () => {
    const totalColumn = columns[2]!;
    const cell = totalColumn.cell as any;
    const { container } = render(cell({ row: { original: mockOrder } } as any));

    const formattedTotal = `$${mockOrder.total.toFixed(2)}`;
    expect(screen.getByText(formattedTotal)).toBeInTheDocument();
    expect(container.querySelector(".font-medium")).toBeInTheDocument();
  });

  it("renders New status badge", () => {
    const statusColumn = columns[3]!;
    const cell = statusColumn.cell as any;
    render(cell({ row: { original: mockOrder } } as any));

    expect(screen.getByText("New")).toBeInTheDocument();
  });

  it("renders Completed status badge", () => {
    const statusColumn = columns[3]!;
    const cell = statusColumn.cell as any;
    const completedOrder = { ...mockOrder, status: "Completed" };
    render(cell({ row: { original: completedOrder } } as any));

    expect(screen.getByText("Completed")).toBeInTheDocument();
  });

  it("renders Cancelled status badge", () => {
    const statusColumn = columns[3]!;
    const cell = statusColumn.cell as any;
    const cancelledOrder = { ...mockOrder, status: "Cancelled" };
    render(cell({ row: { original: cancelledOrder } } as any));

    expect(screen.getByText("Cancelled")).toBeInTheDocument();
  });

  it("has correct header labels", () => {
    expect(columns[0]!.header).toBe("Order ID");
    expect(columns[1]!.header).toBe("Date");
    expect(columns[2]!.header).toBe("Total");
    expect(columns[3]!.header).toBe("Status");
  });

  it("has correct accessor keys", () => {
    expect((columns[0] as any).accessorKey).toBe("id");
    expect((columns[1] as any).accessorKey).toBe("date");
    expect((columns[2] as any).accessorKey).toBe("total");
    expect((columns[3] as any).accessorKey).toBe("status");
  });

  it("actions column has correct id", () => {
    expect(columns[4]!.id).toBe("actions");
  });
});
