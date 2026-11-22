"use client";

import { useComponent } from "@copilotkit/react-core/v2";

import {
  basketSummaryJsonSchema,
  bookCardJsonSchema,
  bookListJsonSchema,
} from "@/a2ui/definitions";
import {
  BasketSummaryView,
  BookCardView,
  BookListView,
} from "@/a2ui/renderers";

/**
 * Registers controlled generative UI components (`useComponent`) that the
 * backend agent can invoke directly as frontend tools to render rich
 * book-store UI cards inside the chat conversation.
 *
 * Registered components:
 * - `showBookCard`      — single book detail card
 * - `showBookList`      — compact list of books (search results / recommendations)
 * - `showBasketSummary` — live basket snapshot
 */
export function useA2UIComponents() {
  useComponent(
    {
      name: "showBookCard",
      description:
        "Render a rich book detail card in chat showing title, authors, price, rating, availability, and an optional description.",
      parameters: bookCardJsonSchema,
      render: BookCardView,
    },
    [],
  );

  useComponent(
    {
      name: "showBookList",
      description:
        "Render a compact list of books in chat, e.g. for search results or recommendations. Each row shows title, authors, and price.",
      parameters: bookListJsonSchema,
      render: BookListView,
    },
    [],
  );

  useComponent(
    {
      name: "showBasketSummary",
      description:
        "Render an inline basket summary in chat showing all current basket items, quantities, and the total price.",
      parameters: basketSummaryJsonSchema,
      render: BasketSummaryView,
    },
    [],
  );
}
