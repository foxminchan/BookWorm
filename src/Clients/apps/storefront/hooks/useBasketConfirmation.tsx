"use client";

import { useState } from "react";

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

interface ConfirmationState {
  isOpen: boolean;
  bookId: string;
  bookTitle?: string;
  price?: number;
  quantity: number;
  resolve?: (confirmed: boolean) => void;
}

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

  const confirm = () => {
    confirmation.resolve?.(true);
    setConfirmation((prev) => ({ ...prev, isOpen: false }));
  };

  const cancel = () => {
    confirmation.resolve?.(false);
    setConfirmation((prev) => ({ ...prev, isOpen: false }));
  };

  const ConfirmationDialog = () => {
    return (
      <Dialog
        open={confirmation.isOpen}
        onOpenChange={(open) => !open && cancel()}
      >
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-3">
              <ShoppingCart className="h-6 w-6 text-blue-600 dark:text-blue-400" />
              Add to Basket
            </DialogTitle>
            <DialogDescription>
              Confirm adding this item to your basket
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-2 py-4">
            {confirmation.bookTitle && (
              <p className="text-sm text-gray-600 dark:text-gray-400">
                <span className="font-medium">Book:</span>{" "}
                {confirmation.bookTitle}
              </p>
            )}
            <p className="text-sm text-gray-600 dark:text-gray-400">
              <span className="font-medium">Quantity:</span>{" "}
              {confirmation.quantity}
            </p>
            {confirmation.price && (
              <p className="text-sm text-gray-600 dark:text-gray-400">
                <span className="font-medium">Price:</span> $
                {(confirmation.price * confirmation.quantity).toFixed(2)}
              </p>
            )}
          </div>

          <DialogFooter className="gap-2 sm:gap-2">
            <Button
              variant="outline"
              onClick={cancel}
              className="flex-1 sm:flex-1"
            >
              <X className="h-4 w-4" />
              Cancel
            </Button>
            <Button onClick={confirm} className="flex-1 sm:flex-1">
              <Check className="h-4 w-4" />
              Confirm
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    );
  };

  return {
    requestConfirmation,
    ConfirmationDialog,
  };
}
