import { Card, CardContent } from "@workspace/ui/components/card";
import { Trash2, Plus, Minus } from "lucide-react";
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
import type { BasketItem as BasketItemType } from "@workspace/types/basket";

type BasketItemProps = {
  item: BasketItemType;
  displayQuantity: number;
  onUpdateQuantity: (id: string, delta: number) => void;
  onRemoveItem: (id: string) => void;
};

export function BasketItem({
  item,
  displayQuantity,
  onUpdateQuantity,
  onRemoveItem,
}: BasketItemProps) {
  return (
    <Card className="border-none shadow-none bg-white/50 dark:bg-gray-800/50 backdrop-blur-sm">
      <CardContent className="p-6">
        <div className="flex gap-6">
          <div className="grow space-y-1">
            <div className="flex justify-between items-start">
              <h3 className="font-serif font-medium text-xl">{item.name}</h3>
              <AlertDialog>
                <AlertDialogTrigger asChild>
                  <button className="text-muted-foreground hover:text-destructive transition-colors">
                    <Trash2 className="size-5" />
                  </button>
                </AlertDialogTrigger>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>Remove item?</AlertDialogTitle>
                    <AlertDialogDescription>
                      Are you sure you want to remove &quot;{item.name}&quot;
                      from your basket?
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>Cancel</AlertDialogCancel>
                    <AlertDialogAction onClick={() => onRemoveItem(item.id)}>
                      Remove
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            </div>
            <p className="text-sm text-muted-foreground">Hardcover</p>
            <div className="flex items-center gap-3 pt-4">
              <div className="flex items-center gap-2">
                <div className="flex items-center border rounded-full px-2 py-1 bg-white dark:bg-gray-900 dark:border-gray-700">
                  <button
                    onClick={() => onUpdateQuantity(item.id, -1)}
                    className="p-1 hover:text-primary transition-colors"
                  >
                    <Minus className="size-4" />
                  </button>
                  <span
                    className={`w-8 text-center font-medium text-sm ${displayQuantity <= 0 ? "text-destructive" : ""}`}
                  >
                    {displayQuantity}
                  </span>
                  <button
                    onClick={() => onUpdateQuantity(item.id, 1)}
                    className="p-1 hover:text-primary transition-colors"
                  >
                    <Plus className="size-4" />
                  </button>
                </div>
              </div>
              <div className="ml-auto text-right">
                {item.priceSale ? (
                  <div className="flex flex-col items-end">
                    <span className="font-bold text-primary">
                      ${(item.priceSale * displayQuantity).toFixed(2)}
                    </span>
                    <span className="text-xs text-muted-foreground line-through decoration-muted-foreground/50">
                      ${(item.price * displayQuantity).toFixed(2)}
                    </span>
                  </div>
                ) : (
                  <span className="font-bold">
                    ${(item.price * displayQuantity).toFixed(2)}
                  </span>
                )}
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
