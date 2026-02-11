"use client";

import { useCallback, useState } from "react";

import { Check, ShoppingCart, X } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@workspace/ui/components/dialog";
import { formatPrice } from "@workspace/utils/format";

type ConfirmationState = {
  isOpen: boolean;
  bookId: string;
  bookTitle?: string;
  price?: number;
  quantity: number;
  resolve?: (confirmed: boolean) => void;
};

export function useBasketConfirmation() {
  const [confirmation, setConfirmation] = useState<ConfirmationState>({
    isOpen: false,
    bookId: "",
    quantity: 1,
  });

  const requestConfirmation = (
    bookId: string,
    quantity: number,
    bookTitle?: string,
    price?: number,
  ): Promise<boolean> => {
    return new Promise((resolve) => {
      setConfirmation({
        isOpen: true,
        bookId,
        bookTitle,
        price,
        quantity,
        resolve,
      });
    });
  };

  const confirm = useCallback(() => {
    confirmation.resolve?.(true);
    setConfirmation((prev) => ({ ...prev, isOpen: false }));
  }, [confirmation.resolve]);

  const cancel = useCallback(() => {
    confirmation.resolve?.(false);
    setConfirmation((prev) => ({ ...prev, isOpen: false }));
  }, [confirmation.resolve]);

  const ConfirmationDialog = () => (
    <Dialog
      open={confirmation.isOpen}
      onOpenChange={(open) => !open && cancel()}
    >
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-3">
            <ShoppingCart
              className="size-6 text-blue-600 dark:text-blue-400"
              aria-hidden="true"
            />
            Add to Basket
          </DialogTitle>
          <DialogDescription>
            Confirm adding this item to your basket
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-2 py-4">
          {confirmation.bookTitle && (
            <p className="text-muted-foreground text-sm">
              <span className="font-medium">Book:</span>{" "}
              {confirmation.bookTitle}
            </p>
          )}
          <p className="text-muted-foreground text-sm">
            <span className="font-medium">Quantity:</span>{" "}
            {confirmation.quantity}
          </p>
          {confirmation.price != null && confirmation.price > 0 && (
            <p className="text-muted-foreground text-sm">
              <span className="font-medium">Price:</span>{" "}
              {formatPrice(confirmation.price * confirmation.quantity)}
            </p>
          )}
        </div>

        <DialogFooter className="gap-2 sm:gap-2">
          <Button
            type="button"
            variant="outline"
            onClick={cancel}
            className="flex-1 sm:flex-1"
          >
            <X className="size-4" aria-hidden="true" />
            Cancel
          </Button>
          <Button type="button" onClick={confirm} className="flex-1 sm:flex-1">
            <Check className="size-4" aria-hidden="true" />
            Confirm
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );

  return {
    requestConfirmation,
    ConfirmationDialog,
  };
}
