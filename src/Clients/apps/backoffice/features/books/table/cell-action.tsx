"use client";

import { useState } from "react";

import Link from "next/link";

import { Edit, Loader2, Trash2 } from "lucide-react";

import useDeleteBook from "@workspace/api-hooks/catalog/books/useDeleteBook";
import type { Book } from "@workspace/types/catalog/books";
import { Button } from "@workspace/ui/components/button";

import { ConfirmDialog } from "@/components/confirm-dialog";

type CellActionProps = {
  book: Book;
};

export function CellAction({ book }: CellActionProps) {
  const [openDelete, setOpenDelete] = useState(false);
  const deleteBookMutation = useDeleteBook();

  return (
    <>
      <div className="flex items-center justify-end gap-2">
        <Button variant="ghost" size="sm" asChild>
          <Link href={`/books/${book.id}`}>
            <Edit className="h-4 w-4" />
          </Link>
        </Button>
        <Button
          variant="ghost"
          size="sm"
          className="text-destructive hover:text-destructive"
          onClick={() => setOpenDelete(true)}
          disabled={deleteBookMutation.isPending}
        >
          {deleteBookMutation.isPending ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <Trash2 className="h-4 w-4" />
          )}
        </Button>
      </div>

      <ConfirmDialog
        open={openDelete}
        onOpenChange={setOpenDelete}
        title="Delete Book"
        description={`Are you sure you want to delete "${book.name ?? "this book"}"? This action cannot be undone.`}
        actionLabel="Delete"
        actionType="delete"
        isLoading={deleteBookMutation.isPending}
        onConfirm={async () => {
          deleteBookMutation.mutate(book.id, {
            onSuccess: () => setOpenDelete(false),
          });
        }}
      />
    </>
  );
}
