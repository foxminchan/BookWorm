"use client";

import { useCallback, useState } from "react";

import Link from "next/link";

import { Edit, Loader2, Trash2 } from "lucide-react";
import { toast } from "sonner";

import useDeleteBook from "@workspace/api-hooks/catalog/books/useDeleteBook";
import type { Book } from "@workspace/types/catalog/books";
import { Button } from "@workspace/ui/components/button";

import { ConfirmDialog } from "@/components/confirm-dialog";

type CellActionProps = Readonly<{
  book: Book;
}>;

export function CellAction({ book }: CellActionProps) {
  const [openDelete, setOpenDelete] = useState(false);
  const deleteBookMutation = useDeleteBook();

  const handleDelete = useCallback(async () => {
    deleteBookMutation.mutate(book.id, {
      onSuccess: () => {
        setOpenDelete(false);
        toast.success("Book has been deleted");
      },
    });
  }, [book.id, deleteBookMutation]);

  return (
    <>
      <div className="flex items-center justify-end gap-2">
        <Button variant="ghost" size="sm" asChild>
          <Link
            href={`/books/${book.id}`}
            aria-label={`Edit ${book.name ?? "book"}`}
          >
            <Edit className="h-4 w-4" aria-hidden="true" />
          </Link>
        </Button>
        <Button
          variant="ghost"
          size="sm"
          className="text-destructive hover:text-destructive"
          onClick={() => setOpenDelete(true)}
          disabled={deleteBookMutation.isPending}
          aria-label={`Delete ${book.name ?? "book"}`}
        >
          {deleteBookMutation.isPending ? (
            <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
          ) : (
            <Trash2 className="h-4 w-4" aria-hidden="true" />
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
        onConfirm={handleDelete}
      />
    </>
  );
}
