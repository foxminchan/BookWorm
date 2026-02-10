"use client";

import { useEffect, useState } from "react";

import { useCopilotAction } from "@copilotkit/react-core";
import { Check } from "lucide-react";

import basketApiClient from "@workspace/api-client/basket/baskets";

import { useBasketConfirmation } from "./useBasketConfirmation";

export function useBasketActions() {
  const { requestConfirmation, ConfirmationDialog } = useBasketConfirmation();
  const [announcement, setAnnouncement] = useState<string>("");

  // Clear announcement after it's been read
  useEffect(() => {
    if (announcement) {
      const timer = setTimeout(() => setAnnouncement(""), 3000);
      return () => clearTimeout(timer);
    }
  }, [announcement]);

  useCopilotAction({
    name: "addToBasket",
    description:
      "Add a book to the user's shopping basket. Use this when the user expresses interest in purchasing a book. Always requires user confirmation before adding.",
    parameters: [
      {
        name: "bookId",
        type: "string",
        description: "The unique identifier of the book to add",
        required: true,
      },
      {
        name: "quantity",
        type: "number",
        description: "Number of copies to add (default 1)",
        required: false,
      },
      {
        name: "bookTitle",
        type: "string",
        description: "The title of the book (for confirmation display)",
        required: false,
      },
      {
        name: "price",
        type: "number",
        description: "The price of the book (for confirmation display)",
        required: false,
      },
    ],
    handler: async ({ bookId, quantity = 1, bookTitle, price }) => {
      // Request user confirmation before adding to basket (Human-in-the-Loop)
      const confirmed = await requestConfirmation(
        bookId,
        quantity,
        bookTitle,
        price,
      );

      if (!confirmed) {
        return {
          success: false,
          cancelled: true,
          message: "Action cancelled by user",
        };
      }

      await basketApiClient.update({
        items: [{ id: bookId, quantity }],
      });

      const message = `Added ${quantity} ${quantity === 1 ? "copy" : "copies"}${bookTitle ? ` of ${bookTitle}` : ""} to basket`;
      setAnnouncement(message);

      return {
        success: true,
        bookId,
        quantity,
        message,
      };
    },
    render: ({ status, result }) => {
      if (status === "executing") {
        return (
          <div className="flex items-center gap-2 rounded-lg border bg-blue-50 p-3 dark:bg-blue-950">
            <div className="h-4 w-4 animate-spin rounded-full border-2 border-blue-600 border-t-transparent" />
            <span className="text-sm">Adding to basket...</span>
          </div>
        );
      }

      if (status === "complete" && result) {
        return (
          <div className="rounded-lg border bg-green-50 p-3 dark:bg-green-950">
            <div className="flex items-center gap-2">
              <Check className="h-5 w-5 text-green-600 dark:text-green-400" />
              <span className="text-sm font-medium">{result.message}</span>
            </div>
          </div>
        );
      }

      return <></>;
    },
  });

  useCopilotAction({
    name: "viewBasket",
    description:
      "Show the current contents of the user's shopping basket with all items and total price",
    parameters: [],
    handler: async () => {
      const data = await basketApiClient.get();

      const totalPrice = data.items.reduce(
        (sum, item) => sum + (item.priceSale || item.price) * item.quantity,
        0,
      );

      return {
        items: data.items || [],
        totalPrice,
        itemCount: data.items?.length || 0,
      };
    },
    render: ({ status, result }) => {
      const formatter = new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
      });

      if (status === "executing") {
        return (
          <div className="flex items-center gap-2 rounded-lg border p-4">
            <div className="border-primary h-4 w-4 animate-spin rounded-full border-2 border-t-transparent" />
            <span className="text-sm">Loading basket...</span>
          </div>
        );
      }

      if (status === "complete" && result) {
        return (
          <div className="space-y-3 rounded-lg border p-4">
            <h3 className="font-semibold">
              Your Basket ({result.itemCount} item
              {result.itemCount === 1 ? "" : "s"})
            </h3>
            {result.items.length === 0 ? (
              <p className="text-muted-foreground text-sm">
                Your basket is empty
              </p>
            ) : (
              <>
                <div className="space-y-2">
                  {result.items.map((item: any) => (
                    <div
                      key={item.id}
                      className="flex items-center gap-3 rounded border p-2"
                    >
                      <div className="flex-1 text-sm">
                        <div className="font-medium">{item.name}</div>
                        <div className="text-muted-foreground text-xs">
                          Qty: {item.quantity} ×{" "}
                          {formatter.format(item.priceSale || item.price)}
                        </div>
                      </div>
                      <div className="font-semibold">
                        {formatter.format(
                          item.quantity * (item.priceSale || item.price),
                        )}
                      </div>
                    </div>
                  ))}
                </div>
                <div className="border-t pt-3">
                  <div className="flex justify-between font-bold">
                    <span>Total:</span>
                    <span>{formatter.format(result.totalPrice)}</span>
                  </div>
                </div>
              </>
            )}
          </div>
        );
      }

      return <></>;
    },
  });

  // Live region component for screen reader announcements
  const LiveRegion = () => (
    <output
      aria-live="polite"
      aria-atomic="true"
      className="sr-only"
    >
      {announcement}
    </output>
  );

  return { ConfirmationDialog, LiveRegion };
}
