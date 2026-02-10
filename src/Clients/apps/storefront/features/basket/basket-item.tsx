import { useCallback, useMemo, useState } from "react";

import { Trash2 } from "lucide-react";

import type { BasketItem as BasketItemType } from "@workspace/types/basket";
import { Button } from "@workspace/ui/components/button";
import { Card, CardContent } from "@workspace/ui/components/card";

import { QuantityControl } from "@/components/quantity-control";
import { RemoveItemDialog } from "@/components/remove-item-dialog";
import { currencyFormatter } from "@/lib/constants";

type BasketItemProps = {
  item: BasketItemType;
  displayQuantity: number;
  onUpdateQuantity: (id: string, delta: number) => void;
  onRemoveItem: (id: string) => void;
};

export default function BasketItem({
  item,
  displayQuantity,
  onUpdateQuantity,
  onRemoveItem,
}: Readonly<BasketItemProps>) {
  const [showRemoveDialog, setShowRemoveDialog] = useState(false);

  const saleTotal = item.priceSale
    ? item.priceSale * displayQuantity
    : undefined;
  const originalTotal = item.price * displayQuantity;
  const totalPrice = saleTotal ?? originalTotal;

  const removeItems = useMemo(
    () => [{ id: item.id, name: item.name || "Unnamed Item" }],
    [item.id, item.name],
  );

  const handleConfirmRemove = useCallback(() => {
    onRemoveItem(item.id);
    setShowRemoveDialog(false);
  }, [item.id, onRemoveItem]);

  return (
    <Card
      className="border-none bg-white/50 shadow-none backdrop-blur-sm dark:bg-gray-800/50"
      role="article"
      aria-label={`${item.name}, quantity ${displayQuantity}, total ${currencyFormatter.format(totalPrice)}`}
    >
      <CardContent className="p-6">
        <div className="flex gap-6">
          <div className="grow space-y-1">
            <div className="flex items-start justify-between">
              <h3 className="font-serif text-xl font-medium">{item.name}</h3>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                onClick={() => setShowRemoveDialog(true)}
                className="text-muted-foreground hover:text-destructive"
                aria-label={`Remove ${item.name} from basket`}
              >
                <Trash2 className="size-5" aria-hidden="true" />
              </Button>
            </div>
            <p className="text-muted-foreground text-sm">Hardcover</p>
            <div className="flex items-center gap-3 pt-4">
              <QuantityControl
                quantity={displayQuantity}
                onDecrease={() => onUpdateQuantity(item.id, -1)}
                onIncrease={() => onUpdateQuantity(item.id, 1)}
                size="md"
              />
              <div className="ml-auto text-right">
                {saleTotal === undefined ? (
                  <>
                    <span className="font-bold">
                      {currencyFormatter.format(originalTotal)}
                    </span>
                    <span className="sr-only">
                      Total: {currencyFormatter.format(originalTotal)}
                    </span>
                  </>
                ) : (
                  <div className="flex flex-col items-end">
                    <span className="text-primary font-bold">
                      {currencyFormatter.format(saleTotal)}
                    </span>
                    <span className="text-muted-foreground decoration-muted-foreground/50 text-xs line-through">
                      {currencyFormatter.format(originalTotal)}
                    </span>
                    <span className="sr-only">
                      Sale price: {currencyFormatter.format(saleTotal)},
                      original price: {currencyFormatter.format(originalTotal)}
                    </span>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </CardContent>
      <RemoveItemDialog
        open={showRemoveDialog}
        onOpenChange={setShowRemoveDialog}
        onConfirm={handleConfirmRemove}
        items={removeItems}
      />
    </Card>
  );
}
