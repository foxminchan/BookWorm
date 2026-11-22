"use client";

import {
  createGenerativeUI,
  standardUI,
} from "@copilotkit/react-core/generative-ui";

import { BOOKWORM_CATALOG_ID } from "./constants";
import {
  a2uiBookCardJsonSchema,
  basketSummaryJsonSchema,
  bookListJsonSchema,
} from "./definitions";
import {
  BasketSummaryView,
  BookCardView,
  BookListView,
  withA2UIAccessibility,
} from "./renderers";

/**
 * Declarative A2UI catalog for the BookWorm storefront.
 *
 * Extends CopilotKit's `standardUI` primitives (Card, Column, Row, Text,
 * Button, Badge, etc.) with BookWorm domain components so the backend agent
 * can compose rich A2UI surfaces via `render_a2ui`.
 */
export const bookwormCatalog = createGenerativeUI({
  id: BOOKWORM_CATALOG_ID,
  extends: [standardUI],
  components: {
    BookCard: {
      description:
        "Rich book detail card showing title, authors, price, optional sale discount, rating, availability, and short description.",
      props: a2uiBookCardJsonSchema,
      component: withA2UIAccessibility(BookCardView),
    },
    BookList: {
      description:
        "Compact list of books for search results or recommendations, showing title, authors, and price per row.",
      props: bookListJsonSchema,
      component: withA2UIAccessibility(BookListView),
    },
    BasketSummary: {
      description:
        "Inline basket summary showing current basket items, quantities, and aggregated total price.",
      props: basketSummaryJsonSchema,
      component: withA2UIAccessibility(BasketSummaryView),
    },
  },
});
