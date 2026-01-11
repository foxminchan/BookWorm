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
      const formatter = new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
      });
      return (
        <div className="text-sm">
          {salePrice ? (
            <span
              aria-label={`Sale price ${formatter.format(salePrice)}, original price ${formatter.format(price)}`}
            >
              <del className="text-gray-400">{formatter.format(price)}</del>
              <span className="ml-2 font-medium">
                {formatter.format(salePrice)}
              </span>
            </span>
          ) : (
            <span>{formatter.format(price)}</span>
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
      const isInStock = status === "InStock";
      return (
        <div
          className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-medium ${
            isInStock
              ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200"
              : "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200"
          }`}
          role="status"
          aria-label={`Book status: ${isInStock ? "In Stock" : "Out of Stock"}`}
        >
          <span
            className={`h-1.5 w-1.5 rounded-full ${
              isInStock
                ? "bg-green-600 dark:bg-green-400"
                : "bg-red-600 dark:bg-red-400"
            }`}
            aria-hidden="true"
          />
          {isInStock ? "In Stock" : "Out of Stock"}
        </div>
      );
    },
  },
  {
    accessorKey: "averageRating",
    header: "Rating",
    cell: ({ row }) => {
      const rating = row.original.averageRating.toFixed(1);
      const reviews = row.original.totalReviews;
      return (
        <div
          className="text-sm"
          aria-label={`Rating ${rating} out of 5 based on ${reviews} ${reviews === 1 ? "review" : "reviews"}`}
        >
          <span aria-hidden="true">
            {rating} ({reviews})
          </span>
        </div>
      );
    },
  },
  {
    id: "actions",
    cell: ({ row }) => <CellAction book={row.original} />,
  },
];
