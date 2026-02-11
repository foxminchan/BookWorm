"use client";

import { useCallback, useState } from "react";

import type { CellContext, ColumnDef } from "@tanstack/react-table";
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { Edit, Trash2 } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import { Input } from "@workspace/ui/components/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@workspace/ui/components/table";

import { ConfirmDialog } from "./confirm-dialog";
import { SimpleTableSkeleton } from "./loading-skeleton";

type BaseItem = {
  id: string;
  name: string | null;
};

type BaseTableProps<T extends BaseItem> = Readonly<{
  title: string;
  description: string;
  items: T[];
  isLoading: boolean;
  onUpdate: (id: string, name: string) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
  isSubmitting?: boolean;
}>;

type SimpleTableMeta = Readonly<{
  editingId: string | null;
  editValue: string;
  isSubmitting: boolean;
  setEditValue: (value: string) => void;
  handleEditSave: (id: string) => Promise<void>;
  cancelEditing: () => void;
  startEditing: (id: string, name: string | null) => void;
  requestDelete: (id: string, name: string | null) => void;
}>;

function getTableMeta(
  context: CellContext<BaseItem, unknown>,
): SimpleTableMeta {
  return context.table.options.meta as SimpleTableMeta;
}

function NumberHeader() {
  return <div className="w-1">#</div>;
}

function NumberCell({ row }: Readonly<CellContext<BaseItem, unknown>>) {
  return <div className="text-muted-foreground w-1">{row.index + 1}</div>;
}

function NameCell(context: Readonly<CellContext<BaseItem, unknown>>) {
  const { editingId, editValue, setEditValue, handleEditSave, cancelEditing } =
    getTableMeta(context);
  const item = context.row.original;

  if (editingId === item.id) {
    return (
      <Input
        value={editValue}
        onChange={(e) => setEditValue(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === "Enter") {
            e.preventDefault();
            handleEditSave(item.id);
          }
          if (e.key === "Escape") {
            cancelEditing();
          }
        }}
        autoFocus
      />
    );
  }

  return <span className="text-foreground">{item.name ?? "N/A"}</span>;
}

function ActionsHeader() {
  return <div className="text-right">Actions</div>;
}

function ActionsCell(context: Readonly<CellContext<BaseItem, unknown>>) {
  const {
    editingId,
    isSubmitting,
    handleEditSave,
    cancelEditing,
    startEditing,
    requestDelete,
  } = getTableMeta(context);
  const item = context.row.original;

  if (editingId === item.id) {
    return (
      <div className="flex gap-2">
        <Button
          size="sm"
          variant="default"
          onClick={() => handleEditSave(item.id)}
          disabled={isSubmitting}
        >
          Save
        </Button>
        <Button size="sm" variant="outline" onClick={cancelEditing}>
          Cancel
        </Button>
      </div>
    );
  }

  return (
    <div className="flex justify-end gap-2">
      <Button
        size="sm"
        variant="ghost"
        onClick={() => startEditing(item.id, item.name)}
        aria-label={`Edit ${item.name ?? "item"}`}
      >
        <Edit className="h-4 w-4" aria-hidden="true" />
      </Button>
      <Button
        size="sm"
        variant="ghost"
        className="text-destructive hover:text-destructive"
        onClick={() => requestDelete(item.id, item.name)}
        aria-label={`Delete ${item.name ?? "item"}`}
      >
        <Trash2 className="h-4 w-4" aria-hidden="true" />
      </Button>
    </div>
  );
}

const TABLE_COLUMNS: ColumnDef<BaseItem>[] = [
  { id: "number", header: NumberHeader, cell: NumberCell },
  { accessorKey: "name", header: "Name", cell: NameCell },
  { id: "actions", header: ActionsHeader, cell: ActionsCell },
];

export function SimpleTable<T extends BaseItem>({
  title,
  description,
  items,
  isLoading,
  onUpdate,
  onDelete,
  isSubmitting = false,
}: BaseTableProps<T>) {
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editValue, setEditValue] = useState("");
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);
  const [deleteConfirmName, setDeleteConfirmName] = useState("");
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const handleEditSave = useCallback(
    async (id: string) => {
      const trimmed = editValue.trim();
      if (trimmed) {
        await onUpdate(id, trimmed);
        setEditingId(null);
      }
    },
    [editValue, onUpdate],
  );

  const handleDeleteConfirm = useCallback(async () => {
    if (!deleteConfirmId) return;

    setDeletingId(deleteConfirmId);
    try {
      await onDelete(deleteConfirmId);
    } finally {
      setDeletingId(null);
      setDeleteConfirmId(null);
    }
  }, [deleteConfirmId, onDelete]);

  const startEditing = useCallback((id: string, name: string | null) => {
    setEditingId(id);
    setEditValue(name ?? "");
  }, []);

  const cancelEditing = useCallback(() => {
    setEditingId(null);
  }, []);

  const requestDelete = useCallback((id: string, name: string | null) => {
    setDeleteConfirmId(id);
    setDeleteConfirmName(name ?? "this item");
  }, []);

  const table = useReactTable({
    data: items,
    columns: TABLE_COLUMNS as ColumnDef<T>[],
    getCoreRowModel: getCoreRowModel(),
    meta: {
      editingId,
      editValue,
      isSubmitting,
      setEditValue,
      handleEditSave,
      cancelEditing,
      startEditing,
      requestDelete,
    } satisfies SimpleTableMeta,
  });

  if (isLoading) {
    return <SimpleTableSkeleton rows={5} columns={TABLE_COLUMNS.length} />;
  }

  return (
    <>
      <Card>
        <CardHeader>
          <CardTitle>{title}</CardTitle>
          <CardDescription>{description}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="rounded-lg border">
            <Table>
              <caption className="sr-only">{description}</caption>
              <TableHeader>
                {table.getHeaderGroups().map((headerGroup) => (
                  <TableRow key={headerGroup.id}>
                    {headerGroup.headers.map((header) => (
                      <TableHead
                        key={header.id}
                        className={header.id === "actions" ? "w-30" : ""}
                        scope="col"
                      >
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
                  table.getRowModel().rows.map((row) => (
                    <TableRow key={row.id}>
                      {row.getVisibleCells().map((cell) => (
                        <TableCell key={cell.id}>
                          {flexRender(
                            cell.column.columnDef.cell,
                            cell.getContext(),
                          )}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell
                      colSpan={TABLE_COLUMNS.length}
                      className="h-24 text-center"
                      role="status"
                    >
                      <span className="text-muted-foreground">
                        No data found
                      </span>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      <ConfirmDialog
        open={deleteConfirmId !== null}
        onOpenChange={(open) => !open && setDeleteConfirmId(null)}
        title="Delete Item"
        description={`Are you sure you want to delete "${deleteConfirmName}"? This action cannot be undone.`}
        actionLabel="Delete"
        actionType="delete"
        isLoading={deletingId !== null}
        onConfirm={handleDeleteConfirm}
      />
    </>
  );
}
