import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@workspace/ui/components/alert-dialog";
import type { ReactNode } from "react";

type RemoveItemDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
  title?: string;
  description?: string | ReactNode;
  items?: Array<{ id: string; name: string }>;
  cancelLabel?: string;
  confirmLabel?: string;
};

export function RemoveItemDialog({
  open,
  onOpenChange,
  onConfirm,
  title = "Remove from Basket?",
  description,
  items = [],
  cancelLabel = "Keep Item",
  confirmLabel = "Remove",
}: RemoveItemDialogProps) {
  const isSingleItem = items.length === 1;
  const isMultipleItems = items.length > 1;

  const defaultDescription = isSingleItem
    ? `You're about to remove ${items[0]?.name} from your basket. This action cannot be undone.`
    : "The following items have been set to zero quantity. Do you want to remove them from your basket?";

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent className="rounded-2xl">
        <AlertDialogHeader>
          <AlertDialogTitle className="text-2xl">{title}</AlertDialogTitle>
          <AlertDialogDescription className="text-base pt-2">
            {description || defaultDescription}
            {isMultipleItems && (
              <div className="mt-4 space-y-2">
                {items.map((item) => (
                  <div key={item.id} className="text-sm text-foreground">
                    • {item.name}
                  </div>
                ))}
              </div>
            )}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <div className="flex gap-3 justify-end">
          <AlertDialogCancel className="rounded-full px-6">
            {cancelLabel}
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={onConfirm}
            className="rounded-full px-6 bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            {confirmLabel}
          </AlertDialogAction>
        </div>
      </AlertDialogContent>
    </AlertDialog>
  );
}
