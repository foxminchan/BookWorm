import type { BasketItem as BasketItemType } from "@workspace/types/basket";

import BasketItem from "./basket-item";

type BasketItemsListProps = {
  items: BasketItemType[];
  modifiedItems: Record<string, number>;
  onUpdateQuantity: (id: string, delta: number) => void;
  onRemoveItem: (id: string) => void;
};

export default function BasketItemsList({
  items,
  modifiedItems,
  onUpdateQuantity,
  onRemoveItem,
}: Readonly<BasketItemsListProps>) {
  return (
    <div className="space-y-6 lg:col-span-8">
      {items.map((item) => {
        const displayQuantity = item.quantity + (modifiedItems[item.id] ?? 0);
        return (
          <BasketItem
            key={item.id}
            item={item}
            displayQuantity={displayQuantity}
            onUpdateQuantity={onUpdateQuantity}
            onRemoveItem={onRemoveItem}
          />
        );
      })}
    </div>
  );
}
