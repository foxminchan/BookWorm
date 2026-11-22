"use client";

import { useMemo, useState } from "react";

import { useRouter } from "next/navigation";

import { useAtomValue } from "jotai";
import { ShoppingBag } from "lucide-react";

import useBasket from "@workspace/api-hooks/basket/useBasket";
import useDeleteBasket from "@workspace/api-hooks/basket/useDeleteBasket";
import useUpdateBasket from "@workspace/api-hooks/basket/useUpdateBasket";
import useCreateOrder from "@workspace/api-hooks/ordering/orders/useCreateOrder";
import {
  basketAtom,
  basketItemsAtom,
} from "@workspace/storefront-shell/atoms/basket-atom";
import { EmptyState } from "@workspace/storefront-shell/components/empty-state";
import { RemoveItemDialog } from "@workspace/storefront-shell/components/remove-item-dialog";
import { DEFAULT_SHIPPING_COST } from "@workspace/storefront-shell/config/app-config";
import type { BasketItem } from "@workspace/types/basket";

import BasketLoadingSkeleton from "@/components/loading-skeleton";
import BasketHeader from "@/features/basket/basket-header";
import BasketItemsList from "@/features/basket/basket-items-list";
import BasketSummary from "@/features/basket/basket-summary";

function getDisplayQuantity(
  item: BasketItem,
  modifiedItems: Record<string, number>,
): number {
  return item.quantity + (modifiedItems[item.id] || 0);
}

export default function BasketPageClient() {
  useBasket();
  const { isPending: isLoadingBasket, data: basket } = useAtomValue(basketAtom);
  const items = useAtomValue(basketItemsAtom);
  const [modifiedItems, setModifiedItems] = useState<Record<string, number>>(
    {},
  );
  const [showRemoveDialog, setShowRemoveDialog] = useState(false);
  const [itemsToRemove, setItemsToRemove] = useState<BasketItem[]>([]);
  const router = useRouter();
  const updateBasket = useUpdateBasket();
  const deleteBasket = useDeleteBasket();
  const createOrder = useCreateOrder();

  const updateQuantity = (id: string, delta: number) => {
    const item = items.find((i: BasketItem) => i.id === id);
    if (!item) return;

    const newQuantity = getDisplayQuantity(item, modifiedItems) + delta;
    if (newQuantity < 0) return;

    const diff = newQuantity - item.quantity;

    setModifiedItems((prev) => {
      const next = { ...prev };
      if (diff === 0) {
        delete next[id];
      } else {
        next[id] = diff;
      }
      return next;
    });
  };

  const applyChanges = (filterZeroQuantity: boolean) => {
    const updatedItems = items
      .filter(
        (item) =>
          !filterZeroQuantity || getDisplayQuantity(item, modifiedItems) > 0,
      )
      .map((item) => ({
        id: item.id,
        quantity: getDisplayQuantity(item, modifiedItems),
      }));

    updateBasket.mutate(
      { request: { items: updatedItems } },
      {
        onSuccess: () => {
          setModifiedItems({});
          setShowRemoveDialog(false);
          setItemsToRemove([]);
        },
      },
    );
  };

  const saveAllQuantities = () => {
    const zeroItems = items.filter(
      (item) => getDisplayQuantity(item, modifiedItems) <= 0,
    );

    if (zeroItems.length > 0) {
      setItemsToRemove(zeroItems);
      setShowRemoveDialog(true);
    } else {
      applyChanges(false);
    }
  };

  const removeItem = (id: string) => {
    const updatedItems = items
      .filter((item) => item.id !== id)
      .map((item) => ({
        id: item.id,
        quantity: item.quantity,
      }));

    updateBasket.mutate(
      { request: { items: updatedItems } },
      {
        onSuccess: () => {
          setModifiedItems((prev) => {
            const next = { ...prev };
            delete next[id];
            return next;
          });
        },
      },
    );
  };

  const clearBasket = () => {
    deleteBasket.mutate(undefined, {
      onSuccess: () => {
        setModifiedItems({});
      },
    });
  };

  const subtotal = useMemo(
    () =>
      items.reduce((acc, item) => {
        const displayQty = getDisplayQuantity(item, modifiedItems);
        return acc + (item.priceSale ?? item.price) * displayQty;
      }, 0),
    [items, modifiedItems],
  );
  const total = subtotal + DEFAULT_SHIPPING_COST;

  const hasChanges = useMemo(
    () => Object.values(modifiedItems).some((diff) => diff !== 0),
    [modifiedItems],
  );

  const handleCheckout = () => {
    if (!basket?.id) return;

    createOrder.mutate(basket.id, {
      onSuccess: (orderId) => {
        router.push(`/checkout/confirmation?orderId=${orderId}`);
      },
    });
  };

  if (isLoadingBasket) {
    return (
      <main className="container mx-auto grow px-4 py-12">
        <BasketLoadingSkeleton />
      </main>
    );
  }

  return (
    <>
      <main className="container mx-auto grow px-4 py-12">
        <BasketHeader
          hasChanges={hasChanges}
          hasItems={items.length > 0}
          onSaveChanges={saveAllQuantities}
          onClearBasket={clearBasket}
        />

        {items.length > 0 ? (
          <div className="mx-auto grid max-w-5xl grid-cols-1 gap-12 lg:grid-cols-12">
            <BasketItemsList
              items={items}
              modifiedItems={modifiedItems}
              onUpdateQuantity={updateQuantity}
              onRemoveItem={removeItem}
            />
            <BasketSummary
              subtotal={subtotal}
              shipping={DEFAULT_SHIPPING_COST}
              total={total}
              isCheckingOut={createOrder.isPending}
              onCheckout={handleCheckout}
            />
          </div>
        ) : (
          <EmptyState
            icon={ShoppingBag}
            title="Your basket is empty"
            description="Looks like you haven't added any books to your collection yet."
            actionLabel="Explore the Collection"
            actionHref="/shop"
          />
        )}
      </main>
      <RemoveItemDialog
        open={showRemoveDialog}
        onOpenChange={setShowRemoveDialog}
        items={itemsToRemove.map((item) => ({
          id: item.id,
          name: item.name || "Unknown Item",
        }))}
        onConfirm={() => applyChanges(true)}
        title="Remove items with zero quantity?"
        cancelLabel="Keep in Basket"
        confirmLabel="Remove Items"
      />
    </>
  );
}
