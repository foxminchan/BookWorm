"use client";

import { useCallback, useMemo, useState } from "react";

import type {
  ColumnDef,
  SortingState,
  VisibilityState,
} from "@tanstack/react-table";
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { ChevronLeft, ChevronRight } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@workspace/ui/components/table";

import { PAGE_SIZES } from "@/lib/constants";

import { FilterTableSkeleton } from "./loading-skeleton";

type FilterTableProps<TData> = Readonly<{
  columns: ColumnDef<TData>[];
  data: TData[];
  title: string;
  description?: string;
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  isLoading: boolean;
  onPaginationChange: (pageIndex: number, pageSize: number) => void;
  onSortingChange?: (sorting: SortingState) => void;
  highlightedId?: string | null;
  getRowId?: (row: TData) => string;
}>;

export function FilterTable<TData>({
  columns,
  data,
  title,
  description,
  totalCount,
  pageIndex,
  pageSize,
  isLoading,
  onPaginationChange,
  onSortingChange,
  highlightedId,
  getRowId,
}: FilterTableProps<TData>) {
  const [sorting, setSorting] = useState<SortingState>([]);
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({});

  const handleSortingChange = useCallback(
    (updaterOrValue: SortingState | ((old: SortingState) => SortingState)) => {
      setSorting((prev) => {
        const newSorting =
          typeof updaterOrValue === "function"
            ? updaterOrValue(prev)
            : updaterOrValue;
        onSortingChange?.(newSorting);
        return newSorting;
      });
    },
    [onSortingChange],
  );

  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
    onSortingChange: handleSortingChange,
    onColumnVisibilityChange: setColumnVisibility,
    state: {
      sorting,
      columnVisibility,
    },
    manualPagination: true,
    manualSorting: true,
  });

  const totalPages = useMemo(
    () => Math.ceil(totalCount / pageSize),
    [totalCount, pageSize],
  );

  const handlePreviousPage = useCallback(() => {
    onPaginationChange(Math.max(0, pageIndex - 1), pageSize);
  }, [onPaginationChange, pageIndex, pageSize]);

  const handleNextPage = useCallback(() => {
    if (pageIndex < totalPages - 1) {
      onPaginationChange(pageIndex + 1, pageSize);
    }
  }, [onPaginationChange, pageIndex, pageSize, totalPages]);

  const handlePageSizeChange = useCallback(
    (newSize: number) => {
      onPaginationChange(0, newSize);
    },
    [onPaginationChange],
  );

  if (isLoading) {
    return (
      <FilterTableSkeleton
        description={description}
        rows={pageSize}
        columns={columns.length}
      />
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        {description && <CardDescription>{description}</CardDescription>}
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <div className="rounded-md border">
            <Table>
              <caption className="sr-only">{description ?? title}</caption>
              <TableHeader>
                {table.getHeaderGroups().map((headerGroup) => (
                  <TableRow key={headerGroup.id}>
                    {headerGroup.headers.map((header) => (
                      <TableHead key={header.id} scope="col">
                        {header.isPlaceholder
                          ? null
                          : flexRender(
                              header.column.columnDef.header,
                              header.getContext(),
                            )}
                      </TableHead>
                    ))}
                  </TableRow>
                ))}
              </TableHeader>
              <TableBody>
                {table.getRowModel().rows.length > 0 ? (
                  table.getRowModel().rows.map((row) => {
                    const rowId = getRowId?.(row.original);
                    const isHighlighted =
                      highlightedId != null && rowId === highlightedId;
                    return (
                      <TableRow
                        key={row.id}
                        data-state={row.getIsSelected() && "selected"}
                        className={
                          isHighlighted
                            ? "bg-green-50 dark:bg-green-950/20"
                            : ""
                        }
                      >
                        {row.getVisibleCells().map((cell) => (
                          <TableCell key={cell.id}>
                            {flexRender(
                              cell.column.columnDef.cell,
                              cell.getContext(),
                            )}
                          </TableCell>
                        ))}
                      </TableRow>
                    );
                  })
                ) : (
                  <TableRow>
                    <TableCell
                      colSpan={columns.length}
                      className="h-24 text-center"
                    >
                      No data found
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>

          {totalCount > 0 && (
            <nav
              className="flex items-center justify-between px-2 py-4"
              aria-label="Table pagination"
            >
              <div className="text-muted-foreground flex-1 text-sm">
                {data.length} of {totalCount} row(s) total.
              </div>
              <div className="flex items-center space-x-6 lg:space-x-8">
                <div className="flex items-center space-x-2">
                  <p className="text-sm font-medium" id="rows-per-page-label">
                    Rows per page
                  </p>
                  <Select
                    value={pageSize.toString()}
                    onValueChange={(value) =>
                      handlePageSizeChange(Number.parseInt(value, 10))
                    }
                  >
                    <SelectTrigger
                      className="h-8 w-17.5"
                      aria-labelledby="rows-per-page-label"
                    >
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {PAGE_SIZES.map((size) => (
                        <SelectItem key={size} value={size.toString()}>
                          {size}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div
                  className="flex w-25 items-center justify-center text-sm font-medium"
                  aria-live="polite"
                >
                  Page {pageIndex + 1} of {Math.max(1, totalPages)}
                </div>
                <div className="flex items-center space-x-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={handlePreviousPage}
                    disabled={pageIndex === 0}
                    aria-label="Go to previous page"
                  >
                    <ChevronLeft className="mr-2 h-4 w-4" aria-hidden="true" />
                    Previous
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={handleNextPage}
                    disabled={pageIndex >= totalPages - 1}
                    aria-label="Go to next page"
                  >
                    Next
                    <ChevronRight className="ml-2 h-4 w-4" aria-hidden="true" />
                  </Button>
                </div>
              </div>
            </nav>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
