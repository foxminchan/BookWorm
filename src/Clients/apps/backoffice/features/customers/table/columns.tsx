import type { CellContext, ColumnDef } from "@tanstack/react-table";

import type { Buyer } from "@workspace/types/ordering/buyers";

import { CellAction } from "./cell-action";

function NameCell({ row }: Readonly<CellContext<Buyer, unknown>>) {
  return (
    <div className="font-medium">{row.getValue<string>("name") || "N/A"}</div>
  );
}

function AddressCell({ row }: Readonly<CellContext<Buyer, unknown>>) {
  return (
    <div className="text-muted-foreground text-sm">
      {row.getValue<string>("address") || "N/A"}
    </div>
  );
}

function ActionsCell({ row }: Readonly<CellContext<Buyer, unknown>>) {
  return <CellAction customer={row.original} />;
}

export const columns: ColumnDef<Buyer>[] = [
  { accessorKey: "name", header: "Name", cell: NameCell },
  { accessorKey: "address", header: "Address", cell: AddressCell },
  { id: "actions", cell: ActionsCell },
];
