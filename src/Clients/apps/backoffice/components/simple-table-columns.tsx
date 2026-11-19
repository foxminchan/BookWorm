"use client";

import { useEffect, useRef } from "react";

import type { CellContext, ColumnDef } from "@tanstack/react-table";
import { Edit, Trash2 } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import { Input } from "@workspace/ui/components/input";

import type { backofficeTableFeatures } from "@/lib/table";

export type BaseItem = {
  id: string;
  name: string | null;
};

export type SimpleTableMeta = Readonly<{
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
  context: CellContext<typeof backofficeTableFeatures, BaseItem, unknown>,
): SimpleTableMeta {
  return context.table.options.meta as SimpleTableMeta;
}

function NumberHeader() {
  return <div className="w-1">#</div>;
}

function NumberCell({
  row,
}: Readonly<CellContext<typeof backofficeTableFeatures, BaseItem, unknown>>) {
  return <div className="text-muted-foreground w-1">{row.index + 1}</div>;
}

function NameCell(
  context: Readonly<
    CellContext<typeof backofficeTableFeatures, BaseItem, unknown>
  >,
) {
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
            if (e.nativeEvent.isComposing || e.keyCode === 229) return;
            void handleEditSave(item.id);
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

function ActionsCell(
  context: Readonly<
    CellContext<typeof backofficeTableFeatures, BaseItem, unknown>
  >,
) {
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
          onClick={() => void handleEditSave(item.id)}
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

export const SIMPLE_TABLE_COLUMNS: ColumnDef<
  typeof backofficeTableFeatures,
  BaseItem
>[] = [
  { id: "number", header: NumberHeader, cell: NumberCell },
  { accessorKey: "name", header: "Name", cell: NameCell },
  { id: "actions", header: ActionsHeader, cell: ActionsCell },
];
