import { Button } from "@workspace/ui/components/button";
import { Check, Trash2 } from "lucide-react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@workspace/ui/components/alert-dialog";

type BasketHeaderProps = {
  hasChanges: boolean;
  hasItems: boolean;
  onSaveChanges: () => void;
  onClearBasket: () => void;
};

export function BasketHeader({
  hasChanges,
  hasItems,
  onSaveChanges,
  onClearBasket,
}: BasketHeaderProps) {
  return (
    <div className="mb-12 max-w-5xl mx-auto relative">
      <h1 className="text-4xl font-serif font-medium mb-4 md:mb-0">
        Your Basket
      </h1>
      <div className="flex items-center gap-3 md:gap-4 md:absolute md:top-12 md:right-4">
        {hasChanges && (
          <Button
            variant="outline"
            className="rounded-full gap-2 border-primary text-primary hover:bg-primary/5 animate-in fade-in slide-in-from-right-4 duration-300 bg-transparent text-sm md:text-base"
            onClick={onSaveChanges}
          >
            <Check className="size-4" />{" "}
            <span className="hidden sm:inline">Save Changes</span>
            <span className="sm:hidden">Save</span>
          </Button>
        )}
        {hasItems && (
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button
                variant="ghost"
                className="text-muted-foreground hover:text-destructive gap-2 text-sm md:text-base"
              >
                <Trash2 className="size-4" />{" "}
                <span className="hidden sm:inline">Clear Basket</span>
                <span className="sm:hidden">Clear</span>
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                <AlertDialogDescription>
                  This will remove all items from your basket. This action
                  cannot be undone.
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction
                  onClick={onClearBasket}
                  className="bg-destructive text-destructive-foreground"
                >
                  Clear Basket
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        )}
      </div>
    </div>
  );
}
