"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { EmptyState } from "@/components/empty-state";
import {
  BasketHeader,
  BasketItemsList,
  BasketSummary,
  BasketLoadingSkeleton,
  RemoveItemsDialog,
} from "@/features/basket";
import { ShoppingBag } from "lucide-react";
import { useState } from "react";
import { useRouter } from "next/navigation";
import type { BasketItem } from "@workspace/types/basket";
import { useAtom, useAtomValue } from "jotai";
import { basketAtom, basketItemsAtom } from "@/atoms/basket-atom";
import useUpdateBasket from "@workspace/api-hooks/basket/useUpdateBasket";
import useDeleteBasket from "@workspace/api-hooks/basket/useDeleteBasket";
import useCreateOrder from "@workspace/api-hooks/ordering/orders/useCreateOrder";

export default function BasketPage() {
  const [{ isPending: isLoadingBasket }] = useAtom(basketAtom);
  const items = useAtomValue(basketItemsAtom);
  const [modifiedItems, setModifiedItems] = useState<Record<string, number>>(
    {},
  );
  const [isCheckingOut, setIsCheckingOut] = useState(false);
  const [showRemoveDialog, setShowRemoveDialog] = useState(false);
  const [itemsToRemove, setItemsToRemove] = useState<BasketItem[]>([]);
  const router = useRouter();
  const updateBasket = useUpdateBasket();
  const deleteBasket = useDeleteBasket();
  const createOrder = useCreateOrder();
  const [{ data: basket }] = useAtom(basketAtom);

  const updateQuantity = (id: string, delta: number) => {
    const item = items.find((i: BasketItem) => i.id === id);
    if (!item) return;

    const newQuantity = item.quantity + (modifiedItems[id] || 0) + delta;
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

  const saveAllQuantities = () => {
    const zeroItems = items.filter((item) => {
      const displayQty = item.quantity + (modifiedItems[item.id] || 0);
      return displayQty <= 0;
    });

    if (zeroItems.length > 0) {
      setItemsToRemove(zeroItems);
      setShowRemoveDialog(true);
    } else {
      applyChanges();
    }
  };

  const applyChanges = () => {
    const updatedItems = items.map((item) => ({
      id: item.id,
      quantity: item.quantity + (modifiedItems[item.id] || 0),
    }));

    updateBasket.mutate(
      { request: { items: updatedItems } },
      {
        onSuccess: () => {
          setModifiedItems({});
          setShowRemoveDialog(false);
        },
      },
    );
  };

  const confirmRemoveZeroItems = () => {
    const updatedItems = items
      .filter((item) => {
        const displayQty = item.quantity + (modifiedItems[item.id] || 0);
        return displayQty > 0;
      })
      .map((item) => ({
        id: item.id,
        quantity: item.quantity + (modifiedItems[item.id] || 0),
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
    deleteBasket.mutate("", {
      onSuccess: () => {
        setModifiedItems({});
      },
    });
  };

  const subtotal = items.reduce((acc, item) => {
    const displayQty = item.quantity + (modifiedItems[item.id] || 0);
    return acc + (item.priceSale ?? item.price) * displayQty;
  }, 0);
  const shipping = 5.0;
  const total = subtotal + shipping;

  const hasChanges = Object.values(modifiedItems).some((diff) => diff !== 0);

  const handleCheckout = async () => {
    if (!basket?.id) return;

    setIsCheckingOut(true);
    createOrder.mutate(basket.id, {
      onSuccess: (orderId) => {
        router.push(`/checkout/confirmation?orderId=${orderId}`);
      },
      onError: () => {
        setIsCheckingOut(false);
      },
    });
  };

  return (
    <div className="min-h-screen flex flex-col bg-[#FDFCFB] dark:bg-gray-950">
      <Header />
      <main className="grow container mx-auto px-4 py-12">
        <BasketHeader
          hasChanges={hasChanges}
          hasItems={items.length > 0}
          onSaveChanges={saveAllQuantities}
          onClearBasket={clearBasket}
        />

        {isLoadingBasket ? (
          <BasketLoadingSkeleton />
        ) : items.length > 0 ? (
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 max-w-5xl mx-auto">
            <BasketItemsList
              items={items}
              modifiedItems={modifiedItems}
              onUpdateQuantity={updateQuantity}
              onRemoveItem={removeItem}
            />
            <BasketSummary
              subtotal={subtotal}
              shipping={shipping}
              total={total}
              isCheckingOut={isCheckingOut}
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
      <RemoveItemsDialog
        open={showRemoveDialog}
        onOpenChange={setShowRemoveDialog}
        itemsToRemove={itemsToRemove}
        onConfirm={confirmRemoveZeroItems}
      />
      <Footer />
    </div>
  );
}
