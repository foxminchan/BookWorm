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
  return (
    <Card className="border-none bg-white/50 shadow-none backdrop-blur-sm dark:bg-gray-800/50">
      <CardContent className="p-6">
        <div className="flex gap-6">
          <div className="grow space-y-1">
            <div className="flex items-start justify-between">
              <h3 className="font-serif text-xl font-medium">{item.name}</h3>
              <Button
                variant="ghost"
                size="icon"
                onClick={() => setShowRemoveDialog(true)}
                className="text-muted-foreground hover:text-destructive"
              >
                <Trash2 className="size-5" />
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
                      ${(item.priceSale * displayQuantity).toFixed(2)}
                    </span>
                    <span className="text-muted-foreground decoration-muted-foreground/50 text-xs line-through">
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
