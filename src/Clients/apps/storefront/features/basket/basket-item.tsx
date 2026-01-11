import { useState } from "react";

import { Trash2 } from "lucide-react";

import type { BasketItem as BasketItemType } from "@workspace/types/basket";
import { Button } from "@workspace/ui/components/button";
import { Card, CardContent } from "@workspace/ui/components/card";

import { QuantityControl } from "@/components/quantity-control";
import { RemoveItemDialog } from "@/components/remove-item-dialog";

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
}: BasketItemProps) {
  const [showRemoveDialog, setShowRemoveDialog] = useState(false);
  const totalPrice = (item.priceSale || item.price) * displayQuantity;
  const formatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  });

  return (
    <Card
      className="border-none bg-white/50 shadow-none backdrop-blur-sm dark:bg-gray-800/50"
      role="article"
      aria-label={`${item.name}, quantity ${displayQuantity}, total ${formatter.format(totalPrice)}`}
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
                {item.priceSale ? (
                  <div className="flex flex-col items-end">
                    <span className="text-primary font-bold">
                      {formatter.format(item.priceSale * displayQuantity)}
                    </span>
                    <span className="text-muted-foreground decoration-muted-foreground/50 text-xs line-through">
                      {formatter.format(item.price * displayQuantity)}
                    </span>
                    <span className="sr-only">
                      Sale price:{" "}
                      {formatter.format(item.priceSale * displayQuantity)},
                      original price:{" "}
                      {formatter.format(item.price * displayQuantity)}
                    </span>
                  </div>
                ) : (
                  <>
                    <span className="font-bold">
                      {formatter.format(item.price * displayQuantity)}
                    </span>
                    <span className="sr-only">
                      Total: {formatter.format(item.price * displayQuantity)}
                    </span>
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
      </CardContent>
      <RemoveItemDialog
        open={showRemoveDialog}
        onOpenChange={setShowRemoveDialog}
        onConfirm={() => {
          onRemoveItem(item.id);
          setShowRemoveDialog(false);
        }}
        items={[{ id: item.id, name: item.name || "Unnamed Item" }]}
      />
    </Card>
  );
}
