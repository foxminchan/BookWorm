import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@workspace/ui/components/alert-dialog";
import type { BasketItem } from "@workspace/types/basket";

type RemoveItemsDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  itemsToRemove: BasketItem[];
  onConfirm: () => void;
};

export function RemoveItemsDialog({
  open,
  onOpenChange,
  itemsToRemove,
  onConfirm,
}: RemoveItemsDialogProps) {
  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Remove items with zero quantity?</AlertDialogTitle>
          <AlertDialogDescription>
            The following items have been set to zero quantity. Do you want to
            remove them from your basket?
            <div className="mt-4 space-y-2">
              {itemsToRemove.map((item) => (
                <div key={item.id} className="text-sm text-foreground">
                  • {item.name}
                </div>
              ))}
            </div>
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Keep in Basket</AlertDialogCancel>
          <AlertDialogAction
            onClick={onConfirm}
            className="bg-destructive text-destructive-foreground"
          >
            Remove Items
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
