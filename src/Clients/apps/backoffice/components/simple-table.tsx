"use client";

import { useCallback, useReducer } from "react";

import type { ColumnDef } from "@tanstack/react-table";
import { flexRender, useTable } from "@tanstack/react-table";
import { toast } from "sonner";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@workspace/ui/components/table";

import { backofficeTableFeatures } from "@/lib/table";

import { ConfirmDialog } from "./confirm-dialog";
import { SimpleTableSkeleton } from "./loading-skeleton";
import {
  type BaseItem,
  SIMPLE_TABLE_COLUMNS,
  type SimpleTableMeta,
} from "./simple-table-columns";

type BaseTableProps<T extends BaseItem> = Readonly<{
  title: string;
  description: string;
  items: T[];
  isLoading: boolean;
  onUpdate: (id: string, name: string) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
  isSubmitting?: boolean;
}>;

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
        try {
          await onUpdate(id, trimmed);
          dispatch({ type: "FINISH_EDIT" });
        } catch (error) {
          console.error("Failed to update table item:", error);
          toast.error("Failed to update item");
        }
      }
    },
    [state.editValue, onUpdate],
  );

  const handleDeleteConfirm = useCallback(async () => {
    if (!state.deleteConfirmId) return;

    dispatch({ type: "START_DELETING" });
    try {
      await onDelete(state.deleteConfirmId);
    } catch (error) {
      console.error("Failed to delete table item:", error);
      toast.error("Failed to delete item");
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

  const table = useTable({
    features: backofficeTableFeatures,
    data: items,
    columns: SIMPLE_TABLE_COLUMNS as ColumnDef<
      typeof backofficeTableFeatures,
      T
    >[],
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
    return (
      <SimpleTableSkeleton rows={5} columns={SIMPLE_TABLE_COLUMNS.length} />
    );
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
                      colSpan={SIMPLE_TABLE_COLUMNS.length}
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
