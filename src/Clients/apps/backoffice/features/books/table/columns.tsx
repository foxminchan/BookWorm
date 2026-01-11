import type { ColumnDef } from "@tanstack/react-table";

import type { Book } from "@workspace/types/catalog/books";

import { CellAction } from "./cell-action";

export const columns: ColumnDef<Book>[] = [
  {
    accessorKey: "name",
    header: "Title",
    cell: ({ row }) => <div className="font-medium">{row.original.name}</div>,
  },
  {
    accessorKey: "authors",
    header: "Authors",
    cell: ({ row }) => (
      <div className="text-sm">
        {row.original.authors.map((author) => author.name).join(", ")}
      </div>
    ),
  },
  {
    accessorKey: "category",
    header: "Category",
    cell: ({ row }) => (
      <div className="text-sm">{row.original.category?.name || "-"}</div>
    ),
  },
  {
    accessorKey: "publisher",
    header: "Publisher",
    cell: ({ row }) => (
      <div className="text-sm">{row.original.publisher?.name || "-"}</div>
    ),
  },
  {
    accessorKey: "price",
    header: "Price",
    cell: ({ row }) => {
      const price = row.original.price;
      const salePrice = row.original.priceSale;
      return (
        <div className="text-sm">
          {salePrice ? (
            <>
              <span className="text-gray-400 line-through">${price}</span>
              <span className="ml-2 font-medium">${salePrice}</span>
            </>
          ) : (
            <span>${price}</span>
          )}
        </div>
      );
    },
  },
  {
    accessorKey: "status",
    header: "Status",
    cell: ({ row }) => {
      const status = row.original.status;
      return (
        <div
          className={`inline-flex rounded-full px-3 py-1 text-xs font-medium ${
            status === "InStock"
              ? "bg-green-100 text-green-800"
              : "bg-red-100 text-red-800"
          }`}
        >
          {status === "InStock" ? "In Stock" : "Out of Stock"}
        </div>
      );
    },
  },
  {
    accessorKey: "averageRating",
    header: "Rating",
    cell: ({ row }) => (
      <div className="text-sm">
        {row.original.averageRating.toFixed(1)} ({row.original.totalReviews})
      </div>
    ),
  },
  {
    id: "actions",
    cell: ({ row }) => <CellAction book={row.original} />,
  },
];
