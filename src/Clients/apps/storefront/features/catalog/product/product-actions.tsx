"use client";

import type React from "react";
import { useState } from "react";
import { Button } from "@workspace/ui/components/button";
import { ShoppingBasket, Loader2 } from "lucide-react";
import { QuantityControl } from "@/components/quantity-control";
import { useSession } from "@/lib/auth-client";
import { useRouter } from "next/navigation";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@workspace/ui/components/dialog";

type ProductActionsProps = {
  quantity: number;
  status: string;
  isAddingToBasket: boolean;
  onAddToBasket: () => void;
  onQuantityChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
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
}: ProductActionsProps) {
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
      <div className="flex flex-col sm:flex-row gap-4">
        {quantity === 0 ? (
          <Button
            size="lg"
            className="rounded-full h-14 text-lg gap-3 w-full sm:w-auto px-10 shadow-lg shadow-primary/20"
            onClick={handleAddToBasket}
            disabled={status !== "InStock" || isAddingToBasket}
          >
            {isAddingToBasket ? (
              <>
                <Loader2 className="size-5 animate-spin" />
                Adding...
              </>
            ) : (
              <>
                <ShoppingBasket className="size-5" /> Add to Basket
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
            showBorder={false}
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
