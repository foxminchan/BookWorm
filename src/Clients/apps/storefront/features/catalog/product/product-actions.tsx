"use client";

import { type ChangeEvent, useState } from "react";

import { useRouter } from "next/navigation";

import { Loader2, ShoppingBasket } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@workspace/ui/components/dialog";

import { QuantityControl } from "@/components/quantity-control";
import { useSession } from "@/lib/auth-client";

type ProductActionsProps = {
  quantity: number;
  status: string;
  isAddingToBasket: boolean;
  onAddToBasket: () => void;
  onQuantityChange: (e: ChangeEvent<HTMLInputElement>) => void;
  onDecrease: () => void;
  onIncrease: () => void;
};

export default function ProductActions({
  quantity,
  status,
  isAddingToBasket,
  onAddToBasket,
  onQuantityChange,
  onDecrease,
  onIncrease,
}: Readonly<ProductActionsProps>) {
  const { data: session } = useSession();
  const router = useRouter();
  const [showLoginDialog, setShowLoginDialog] = useState(false);

  const handleAddToBasket = () => {
    if (!session?.user) {
      setShowLoginDialog(true);
      return;
    }
    onAddToBasket();
  };

  return (
    <>
      <div className="flex flex-col gap-4 sm:flex-row">
        {quantity === 0 ? (
          <Button
            size="lg"
            className="shadow-primary/20 h-14 w-full gap-3 rounded-full px-10 text-lg shadow-lg sm:w-auto"
            onClick={handleAddToBasket}
            disabled={status !== "InStock" || isAddingToBasket}
          >
            {isAddingToBasket ? (
              <>
                <Loader2 className="size-5 animate-spin" aria-hidden="true" />
                Adding...
              </>
            ) : (
              <>
                <ShoppingBasket className="size-5" aria-hidden="true" /> Add to
                Basket
              </>
            )}
          </Button>
        ) : (
          <QuantityControl
            quantity={quantity}
            onDecrease={onDecrease}
            onIncrease={onIncrease}
            onQuantityChange={onQuantityChange}
            variant="input"
            size="lg"
          />
        )}
      </div>

      <Dialog open={showLoginDialog} onOpenChange={setShowLoginDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Login Required</DialogTitle>
            <DialogDescription>
              You need to be logged in to add items to your basket. Please login
              or register to continue shopping.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter className="gap-2 sm:gap-0">
            <Button variant="outline" onClick={() => setShowLoginDialog(false)}>
              Cancel
            </Button>
            <Button onClick={() => router.push("/login")}>Login</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
