import type { ColumnDef } from "@tanstack/react-table";

import type { Buyer } from "@workspace/types/ordering/buyers";

import { CellAction } from "./cell-action";

export const columns: ColumnDef<Buyer>[] = [
  {
    accessorKey: "name",
    header: "Name",
    cell: ({ row }) => (
      <div className="font-medium">{row.getValue("name") || "N/A"}</div>
    ),
  },
  {
    accessorKey: "address",
    header: "Address",
    cell: ({ row }) => (
      <div className="text-muted-foreground text-sm">
        {row.getValue("address") || "N/A"}
      </div>
    ),
  },
  {
    id: "actions",
    cell: ({ row }) => <CellAction customer={row.original} />,
  },
];
