"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { Button } from "@workspace/ui/components/button";
import { Card, CardContent } from "@workspace/ui/components/card";
import { Separator } from "@workspace/ui/components/separator";
import { Skeleton } from "@workspace/ui/components/skeleton";
import {
  Trash2,
  Plus,
  Minus,
  ArrowRight,
  ShoppingBag,
  Check,
  Loader2,
} from "lucide-react";
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
import Link from "next/link";
import { useState } from "react";
import { useRouter } from "next/navigation";
import type { BasketItem } from "@workspace/types/basket";
import { useAtom, useAtomValue } from "jotai";
import { basketAtom, basketItemsAtom } from "@/lib/basket-context";

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
    // TODO: Implement with mutation to update basket
    setModifiedItems({});
    setShowRemoveDialog(false);
  };

  const confirmRemoveZeroItems = () => {
    // TODO: Implement with mutation to remove items from basket
    setModifiedItems({});
    setShowRemoveDialog(false);
    setItemsToRemove([]);
  };

  const removeItem = (id: string) => {
    // TODO: Implement with mutation
    setModifiedItems((prev) => {
      const next = { ...prev };
      delete next[id];
      return next;
    });
  };

  const clearBasket = () => {
    // TODO: Implement with mutation
    setModifiedItems({});
  };

  const subtotal = items.reduce((acc, item) => {
    const displayQty = item.quantity + (modifiedItems[item.id] || 0);
    return acc + (item.priceSale ?? item.price) * displayQty;
  }, 0);
  const shipping = 5.0;
  const total = subtotal + shipping;

  const hasChanges = Object.values(modifiedItems).some((diff) => diff !== 0);

  const handleCheckout = async () => {
    setIsCheckingOut(true);
    // Simulate processing delay
    await new Promise((resolve) => setTimeout(resolve, 1500));
    router.push("/checkout/confirmation");
  };

  return (
    <div className="min-h-screen flex flex-col bg-[#FDFCFB]">
      <Header />
      <main className="grow container mx-auto px-4 py-12">
        <div className="mb-12 max-w-5xl mx-auto relative">
          <h1 className="text-4xl font-serif font-medium mb-4 md:mb-0">
            Your Basket
          </h1>
          <div className="flex items-center gap-3 md:gap-4 md:absolute md:top-12 md:right-4">
            {hasChanges && (
              <Button
                variant="outline"
                className="rounded-full gap-2 border-primary text-primary hover:bg-primary/5 animate-in fade-in slide-in-from-right-4 duration-300 bg-transparent text-sm md:text-base"
                onClick={saveAllQuantities}
              >
                <Check className="size-4" />{" "}
                <span className="hidden sm:inline">Save Changes</span>
                <span className="sm:hidden">Save</span>
              </Button>
            )}
            {items.length > 0 && (
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
                    <AlertDialogTitle>
                      Are you absolutely sure?
                    </AlertDialogTitle>
                    <AlertDialogDescription>
                      This will remove all items from your basket. This action
                      cannot be undone.
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>Cancel</AlertDialogCancel>
                    <AlertDialogAction
                      onClick={clearBasket}
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

        {isLoadingBasket ? (
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 max-w-5xl mx-auto">
            <div className="lg:col-span-8 space-y-6">
              {[1, 2].map((i) => (
                <Card key={i} className="border-none shadow-none bg-white/50">
                  <CardContent className="p-6">
                    <div className="flex gap-6">
                      <div className="grow space-y-4">
                        <div className="flex justify-between items-start">
                          <Skeleton className="h-6 w-48" />
                          <Skeleton className="h-5 w-5 rounded-full" />
                        </div>
                        <Skeleton className="h-4 w-20" />
                        <div className="flex items-center gap-3 pt-4">
                          <Skeleton className="h-10 w-32" />
                          <Skeleton className="h-6 w-24 ml-auto" />
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>

            <div className="lg:col-span-4">
              <Card className="border-none shadow-none bg-white p-8">
                <Skeleton className="h-8 w-40 mb-6" />
                <div className="space-y-4">
                  <div className="flex justify-between">
                    <Skeleton className="h-4 w-20" />
                    <Skeleton className="h-4 w-16" />
                  </div>
                  <div className="flex justify-between">
                    <Skeleton className="h-4 w-20" />
                    <Skeleton className="h-4 w-16" />
                  </div>
                  <Separator />
                  <div className="flex justify-between">
                    <Skeleton className="h-6 w-16" />
                    <Skeleton className="h-6 w-20" />
                  </div>
                  <Skeleton className="h-12 w-full rounded-full" />
                  <Skeleton className="h-3 w-48 mx-auto" />
                </div>
              </Card>
            </div>
          </div>
        ) : items.length > 0 ? (
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 max-w-5xl mx-auto">
            {/* Basket Items */}
            <div className="lg:col-span-8 space-y-6">
              {items.map((item) => {
                const displayQuantity =
                  item.quantity + (modifiedItems[item.id] || 0);

                return (
                  <Card
                    key={item.id}
                    className="border-none shadow-none bg-white/50 backdrop-blur-sm"
                  >
                    <CardContent className="p-6">
                      <div className="flex gap-6">
                        <div className="grow space-y-1">
                          <div className="flex justify-between items-start">
                            <h3 className="font-serif font-medium text-xl">
                              {item.name}
                            </h3>
                            <AlertDialog>
                              <AlertDialogTrigger asChild>
                                <button className="text-muted-foreground hover:text-destructive transition-colors">
                                  <Trash2 className="size-5" />
                                </button>
                              </AlertDialogTrigger>
                              <AlertDialogContent>
                                <AlertDialogHeader>
                                  <AlertDialogTitle>
                                    Remove item?
                                  </AlertDialogTitle>
                                  <AlertDialogDescription>
                                    Are you sure you want to remove &quot;
                                    {item.name}&quot; from your basket?
                                  </AlertDialogDescription>
                                </AlertDialogHeader>
                                <AlertDialogFooter>
                                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                                  <AlertDialogAction
                                    onClick={() => removeItem(item.id)}
                                  >
                                    Remove
                                  </AlertDialogAction>
                                </AlertDialogFooter>
                              </AlertDialogContent>
                            </AlertDialog>
                          </div>
                          <p className="text-sm text-muted-foreground">
                            Hardcover
                          </p>
                          <div className="flex items-center gap-3 pt-4">
                            <div className="flex items-center gap-2">
                              <div className="flex items-center border rounded-full px-2 py-1 bg-white">
                                <button
                                  onClick={() => updateQuantity(item.id, -1)}
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
                                  onClick={() => updateQuantity(item.id, 1)}
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
                                    $
                                    {(item.priceSale * displayQuantity).toFixed(
                                      2,
                                    )}
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
              })}
            </div>

            {/* Summary */}
            <div className="lg:col-span-4">
              <Card className="border-none shadow-none bg-white p-8 sticky top-32">
                <h2 className="text-2xl font-serif font-medium mb-6">
                  Order Summary
                </h2>
                <div className="space-y-4">
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Subtotal</span>
                    <span className="font-medium">${subtotal.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Shipping</span>
                    <span className="font-medium">${shipping.toFixed(2)}</span>
                  </div>
                  <Separator />
                  <div className="flex justify-between text-lg font-bold">
                    <span>Total</span>
                    <span className="text-primary">${total.toFixed(2)}</span>
                  </div>
                  <Button
                    onClick={handleCheckout}
                    disabled={isCheckingOut}
                    className="w-full h-12 rounded-full text-lg mt-4 group disabled:opacity-80"
                  >
                    {isCheckingOut ? (
                      <>
                        <Loader2 className="mr-2 size-5 animate-spin" />
                        Processing...
                      </>
                    ) : (
                      <>
                        Checkout{" "}
                        <ArrowRight className="ml-2 size-5 group-hover:translate-x-1 transition-transform" />
                      </>
                    )}
                  </Button>
                  <p className="text-center text-xs text-muted-foreground mt-4 italic">
                    Taxes calculated at checkout
                  </p>
                </div>
              </Card>
            </div>
          </div>
        ) : (
          <div className="text-center py-24 space-y-6">
            <div className="size-20 bg-secondary rounded-full flex items-center justify-center mx-auto text-muted-foreground">
              <ShoppingBag className="size-10" />
            </div>
            <h2 className="text-2xl font-serif font-medium">
              Your basket is empty
            </h2>
            <p className="text-muted-foreground">
              Looks like you haven&apos;t added any books to your collection
              yet.
            </p>
            <Button asChild rounded-full>
              <Link href="/shop">Explore the Collection</Link>
            </Button>
          </div>
        )}
      </main>
      <AlertDialog open={showRemoveDialog} onOpenChange={setShowRemoveDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              Remove items with zero quantity?
            </AlertDialogTitle>
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
              onClick={confirmRemoveZeroItems}
              className="bg-destructive text-destructive-foreground"
            >
              Remove Items
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
      <Footer />
    </div>
  );
}
