import type { CellContext, ColumnDef } from "@tanstack/react-table";

import type { Book } from "@workspace/types/catalog/books";
import { formatPrice } from "@workspace/utils/format";

import { CellAction } from "./cell-action";

function TitleCell({ row }: Readonly<CellContext<Book, unknown>>) {
  return <div className="font-medium">{row.original.name}</div>;
}

function AuthorsCell({ row }: Readonly<CellContext<Book, unknown>>) {
  return (
    <div className="text-sm">
      {row.original.authors.map((author) => author.name).join(", ")}
    </div>
  );
}

function CategoryCell({ row }: Readonly<CellContext<Book, unknown>>) {
  return <div className="text-sm">{row.original.category?.name ?? "-"}</div>;
}

function PublisherCell({ row }: Readonly<CellContext<Book, unknown>>) {
  return <div className="text-sm">{row.original.publisher?.name ?? "-"}</div>;
}

function PriceCell({ row }: Readonly<CellContext<Book, unknown>>) {
  const { price, priceSale } = row.original;
  return (
    <div className="text-sm">
      {priceSale ? (
        <span
          aria-label={`Sale price ${formatPrice(priceSale)}, original price ${formatPrice(price)}`}
        >
          <del className="text-muted-foreground">{formatPrice(price)}</del>
          <span className="ml-2 font-medium">{formatPrice(priceSale)}</span>
        </span>
      ) : (
        <span>{formatPrice(price)}</span>
      )}
    </div>
  );
}

function StatusCell({ row }: Readonly<CellContext<Book, unknown>>) {
  const isInStock = row.original.status === "InStock";
  return (
    <output
      className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-medium ${
        isInStock
          ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200"
          : "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200"
      }`}
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
    </output>
  );
}

function RatingCell({ row }: Readonly<CellContext<Book, unknown>>) {
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
}

function ActionsCell({ row }: Readonly<CellContext<Book, unknown>>) {
  return <CellAction book={row.original} />;
}

export const columns: ColumnDef<Book>[] = [
  { accessorKey: "name", header: "Title", cell: TitleCell },
  { accessorKey: "authors", header: "Authors", cell: AuthorsCell },
  { accessorKey: "category", header: "Category", cell: CategoryCell },
  { accessorKey: "publisher", header: "Publisher", cell: PublisherCell },
  { accessorKey: "price", header: "Price", cell: PriceCell },
  { accessorKey: "status", header: "Status", cell: StatusCell },
  { accessorKey: "averageRating", header: "Rating", cell: RatingCell },
  { id: "actions", cell: ActionsCell },
];
