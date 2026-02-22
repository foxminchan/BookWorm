"use client";

import { useCallback, useEffect, useReducer, useRef } from "react";

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
  const inputRef = useRef<HTMLInputElement>(null);
  const isEditing = editingId === item.id;

  useEffect(() => {
    if (isEditing) {
      inputRef.current?.focus();
    }
  }, [isEditing]);

  if (isEditing) {
    return (
      <Input
        ref={inputRef}
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

type TableState = {
  editingId: string | null;
  editValue: string;
  deleteConfirmId: string | null;
  deleteConfirmName: string;
  deletingId: string | null;
};

type TableAction =
  | { type: "START_EDIT"; id: string; name: string }
  | { type: "SET_EDIT_VALUE"; value: string }
  | { type: "CANCEL_EDIT" }
  | { type: "FINISH_EDIT" }
  | { type: "REQUEST_DELETE"; id: string; name: string }
  | { type: "START_DELETING" }
  | { type: "FINISH_DELETE" };

const initialTableState: TableState = {
  editingId: null,
  editValue: "",
  deleteConfirmId: null,
  deleteConfirmName: "",
  deletingId: null,
};

function tableReducer(state: TableState, action: TableAction): TableState {
  switch (action.type) {
    case "START_EDIT":
      return { ...state, editingId: action.id, editValue: action.name };
    case "SET_EDIT_VALUE":
      return { ...state, editValue: action.value };
    case "CANCEL_EDIT":
    case "FINISH_EDIT":
      return { ...state, editingId: null };
    case "REQUEST_DELETE":
      return {
        ...state,
        deleteConfirmId: action.id,
        deleteConfirmName: action.name,
      };
    case "START_DELETING":
      return { ...state, deletingId: state.deleteConfirmId };
    case "FINISH_DELETE":
      return { ...state, deletingId: null, deleteConfirmId: null };
    default:
      return state;
  }
}

export function SimpleTable<T extends BaseItem>({
  title,
  description,
  items,
  isLoading,
  onUpdate,
  onDelete,
  isSubmitting = false,
}: BaseTableProps<T>) {
  const [state, dispatch] = useReducer(tableReducer, initialTableState);

  const setEditValue = useCallback(
    (value: string) => dispatch({ type: "SET_EDIT_VALUE", value }),
    [],
  );

  const handleEditSave = useCallback(
    async (id: string) => {
      const trimmed = state.editValue.trim();
      if (trimmed) {
        await onUpdate(id, trimmed);
        dispatch({ type: "FINISH_EDIT" });
      }
    },
    [state.editValue, onUpdate],
  );

  const handleDeleteConfirm = useCallback(async () => {
    if (!state.deleteConfirmId) return;

    dispatch({ type: "START_DELETING" });
    try {
      await onDelete(state.deleteConfirmId);
    } finally {
      dispatch({ type: "FINISH_DELETE" });
    }
  }, [state.deleteConfirmId, onDelete]);

  const startEditing = useCallback(
    (id: string, name: string | null) =>
      dispatch({ type: "START_EDIT", id, name: name ?? "" }),
    [],
  );

  const cancelEditing = useCallback(
    () => dispatch({ type: "CANCEL_EDIT" }),
    [],
  );

  const requestDelete = useCallback(
    (id: string, name: string | null) =>
      dispatch({
        type: "REQUEST_DELETE",
        id,
        name: name ?? "this item",
      }),
    [],
  );

  const table = useReactTable({
    data: items,
    columns: TABLE_COLUMNS as ColumnDef<T>[],
    getCoreRowModel: getCoreRowModel(),
    meta: {
      editingId: state.editingId,
      editValue: state.editValue,
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
        open={state.deleteConfirmId !== null}
        onOpenChange={(open) => !open && dispatch({ type: "FINISH_DELETE" })}
        title="Delete Item"
        description={`Are you sure you want to delete "${state.deleteConfirmName}"? This action cannot be undone.`}
        actionLabel="Delete"
        actionType="delete"
        isLoading={state.deletingId !== null}
        onConfirm={handleDeleteConfirm}
      />
    </>
  );
}
