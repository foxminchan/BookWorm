import type { CellContext, ColumnDef } from "@tanstack/react-table";

import type { Buyer } from "@workspace/types/ordering/buyers";

import { backofficeTableFeatures } from "@/lib/table";

import { CellAction } from "./cell-action";

type BuyerCellContext = CellContext<
  typeof backofficeTableFeatures,
  Buyer,
  unknown
>;

function NameCell({ row }: Readonly<BuyerCellContext>) {
  return (
    <div className="font-medium">{row.getValue<string>("name") || "N/A"}</div>
  );
}

function AddressCell({ row }: Readonly<BuyerCellContext>) {
  return (
    <div className="text-muted-foreground text-sm">
      {row.getValue<string>("address") || "N/A"}
    </div>
  );
}

function ActionsCell({ row }: Readonly<BuyerCellContext>) {
  return <CellAction customer={row.original} />;
}

export const columns: ColumnDef<typeof backofficeTableFeatures, Buyer>[] = [
  { accessorKey: "name", header: "Name", cell: NameCell },
  { accessorKey: "address", header: "Address", cell: AddressCell },
  { id: "actions", cell: ActionsCell },
];
