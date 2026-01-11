import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import type { Buyer } from "@workspace/types/ordering/buyers";

import { createMockCustomer } from "@/__tests__/factories";
import { columns } from "@/features/customers/table/columns";

describe("Customers Table Columns", () => {
  const mockCustomer = createMockCustomer();

  it("has correct column count", () => {
    expect(columns).toHaveLength(3); // name, address, actions
  });

  it("renders customer name with font-medium", () => {
    const nameColumn = columns[0]!;
    const cell = nameColumn.cell as any;
    const { container } = render(
      cell({
        row: { original: mockCustomer, getValue: () => mockCustomer.name },
      } as any),
    );

    expect(screen.getByText(mockCustomer.name)).toBeInTheDocument();
    expect(container.querySelector(".font-medium")).toBeInTheDocument();
  });

  it("renders N/A when name is empty", () => {
    const nameColumn = columns[0]!;
    const cell = nameColumn.cell as any;
    const customerWithoutName = { ...mockCustomer, name: "" };
    render(
      cell({
        row: { original: customerWithoutName, getValue: () => "" },
      } as any),
    );

    expect(screen.getByText("N/A")).toBeInTheDocument();
  });

  it("renders customer address", () => {
    const addressColumn = columns[1]!;
    const cell = addressColumn.cell as any;
    const { container } = render(
      cell({
        row: {
          original: mockCustomer,
          getValue: () => mockCustomer.address,
        },
      } as any),
    );

    expect(screen.getByText(mockCustomer.address)).toBeInTheDocument();
    expect(
      container.querySelector(".text-muted-foreground"),
    ).toBeInTheDocument();
  });

  it("renders N/A when address is empty", () => {
    const addressColumn = columns[1]!;
    const cell = addressColumn.cell as any;
    const customerWithoutAddress = { ...mockCustomer, address: "" };
    render(
      cell({
        row: { original: customerWithoutAddress, getValue: () => "" },
      } as any),
    );

    expect(screen.getByText("N/A")).toBeInTheDocument();
  });

  it("has correct header labels", () => {
    expect(columns[0]!.header).toBe("Name");
    expect(columns[1]!.header).toBe("Address");
  });

  it("has correct accessor keys", () => {
    expect((columns[0] as any).accessorKey).toBe("name");
    expect((columns[1] as any).accessorKey).toBe("address");
  });

  it("actions column has correct id", () => {
    expect(columns[2]!.id).toBe("actions");
  });
});
