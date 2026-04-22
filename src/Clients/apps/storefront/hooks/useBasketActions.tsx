import { useCallback, useEffect, useRef, useState } from "react";

import { useCopilotAction } from "@copilotkit/react-core";

import basketApiClient from "@workspace/api-client/basket/baskets";
import type { BasketItem } from "@workspace/types/basket";
import { formatPrice } from "@workspace/utils/format";

import { BasketConfirmDialog } from "@/components/chat-hitl/BasketConfirmDialog";

import { useChatAgentState } from "./useChatAgentState";

const MAX_RETRIES = 3;

/**
 * Provides CopilotKit actions for basket management with HITL approval via
 * `renderAndWaitForResponse`. Includes cancel-on-new-message behavior,
 * price re-fetch at confirmation time, and up to 3 retries on network failure.
 */
export function useBasketActions() {
  const [announcement, setAnnouncement] = useState<string>("");
  const clearTimerRef = useRef<ReturnType<typeof setTimeout>>(null);
  const { isAgentRunning } = useChatAgentState();

  // Holds the pending respond callback from renderAndWaitForResponse.
  // When the agent starts a new turn this ref is used to auto-dismiss.
  const pendingRespond = useRef<
    ((response: { approved: boolean; quantity?: number }) => void) | null
  >(null);

  // Cancel pending approval when agent starts processing a new message
  useEffect(() => {
    if (isAgentRunning && pendingRespond.current) {
      pendingRespond.current({ approved: false });
      pendingRespond.current = null;
    }
  }, [isAgentRunning]);

  const announce = useCallback((message: string) => {
    if (clearTimerRef.current) {
      clearTimeout(clearTimerRef.current);
    }
    setAnnouncement(message);
    clearTimerRef.current = setTimeout(() => setAnnouncement(""), 3000);
  }, []);

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
    renderAndWaitForResponse({ args, respond, status }) {
      const { bookId, quantity = 1, bookTitle, price } = args;

      // Store the respond callback for cancel-on-new-message
      if (status === "inProgress" && respond) {
        pendingRespond.current = respond;
      }

      if (status === "complete") {
        return <></>;
      }

      return (
        <BasketConfirmDialog
          bookId={String(bookId ?? "")}
          bookTitle={bookTitle != null ? String(bookTitle) : undefined}
          unitPrice={price != null ? Number(price) : undefined}
          initialQuantity={quantity != null ? Number(quantity) : 1}
          onConfirm={async (confirmedQuantity) => {
            pendingRespond.current = null;

            // Re-fetch current price to detect price changes before committing
            let effectivePrice = price != null ? Number(price) : undefined;
            try {
              const basketData = await basketApiClient.get();
              const existingItem = basketData.items.find(
                (item) => item.id === bookId,
              );
              if (existingItem) {
                effectivePrice = existingItem.priceSale ?? existingItem.price;
              }
            } catch {
              // Price re-fetch is best-effort; proceed with the original price
            }

            // Attempt basket update with up to MAX_RETRIES retries
            let attempt = 0;
            let lastError: unknown = null;
            while (attempt < MAX_RETRIES) {
              try {
                await basketApiClient.update({
                  items: [{ id: String(bookId), quantity: confirmedQuantity }],
                });

                const bookSuffix = bookTitle ? ` of ${bookTitle}` : "";
                const message = `Added ${confirmedQuantity} ${confirmedQuantity === 1 ? "copy" : "copies"}${bookSuffix} to basket`;
                announce(message);

                respond?.({
                  approved: true,
                  quantity: confirmedQuantity,
                });
                return;
              } catch (err) {
                lastError = err;
                attempt++;
              }
            }

            // All retries exhausted
            announce(
              `Failed to add to basket after ${MAX_RETRIES} attempts. Please try again.`,
            );
            respond?.({ approved: false });
          }}
          onDismiss={() => {
            pendingRespond.current = null;
            respond?.({ approved: false });
          }}
        />
      );
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
      if (status === "executing") {
        return (
          <div className="flex items-center gap-2 rounded-lg border p-4">
            <div className="border-primary size-4 animate-spin rounded-full border-2 border-t-transparent" />
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
                  {result.items.map((item: BasketItem) => (
                    <div
                      key={item.id}
                      className="flex items-center gap-3 rounded border p-2"
                    >
                      <div className="flex-1 text-sm">
                        <div className="font-medium">{item.name}</div>
                        <div className="text-muted-foreground text-xs">
                          Qty: {item.quantity} ×{" "}
                          {formatPrice(item.priceSale ?? item.price)}
                        </div>
                      </div>
                      <div className="font-semibold">
                        {formatPrice(
                          item.quantity * (item.priceSale ?? item.price),
                        )}
                      </div>
                    </div>
                  ))}
                </div>
                <div className="border-t pt-3">
                  <div className="flex justify-between font-bold">
                    <span>Total:</span>
                    <span>{formatPrice(result.totalPrice)}</span>
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

  // Live region for screen reader announcements
  const liveRegion = (
    <output aria-live="polite" aria-atomic="true" className="sr-only">
      {announcement}
    </output>
  );

  return { liveRegion };
}
