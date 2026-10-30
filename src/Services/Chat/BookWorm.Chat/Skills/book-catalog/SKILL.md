---
name: book-catalog
description: Search and recommend books from BookWorm's catalog. Use when customers ask about finding books, getting personalized recommendations, exploring genres, discovering authors, or comparing titles.
metadata:
  author: Nhan Nguyen
  version: "1.0"
license: MIT
---

# Book Catalog

## Search Capabilities

Use the `search_catalog` tool to find books. The catalog supports:

| Filter           | Examples                                  |
|------------------|-------------------------------------------|
| Title keyword    | "clean code", "dune"                      |
| Author name      | "Martin Fowler", "Frank Herbert"          |
| Genre / category | "science fiction", "software engineering" |
| Price range      | min/max price filters                     |
| Rating           | minimum star rating                       |
| Availability     | in-stock only                             |

## Recommendation Strategy

1. **Clarify preferences** — Ask about genre, favorite authors, reading level, or purpose (gift, self-learning,
   leisure).
2. **Search first** — Always use `search_catalog` before recommending; never invent titles.
3. **Rank results** — Prioritize fit to the customer's stated preferences. Use ratings and review
   counts as secondary reader-feedback signals when available; do not treat them as evidence of
   sales or popularity.
4. **Provide context** — Include author, genre, a one-sentence description, and price.
5. **Suggest alternatives** — Offer 2–3 suitable options when possible, with concise reasons
   grounded in the stated preferences and catalog details.

Apply the same recommendation criteria to every customer. Do not infer demographic traits or use
stereotypes to decide what someone should read. Distinguish catalog facts from recommendation
judgments, and say when a requested detail is unavailable.

## Response Format

- Present books as a numbered or bulleted list.
- Include: **Title** — _Author_ — Genre — ⭐ Rating — 💲 Price.
- If no results, say so and offer to broaden the search.

## Handoff Rules

- Policy or account questions → hand off to QAAgent.
- Handle emotional book feedback directly: acknowledge the customer's sentiment naturally and
  adapt the response without exposing an internal sentiment label unless requested.
- For long or complex book requests, preserve all questions, preferences, and constraints while
  condensing the request internally; follow the BookAgent planning workflow when applicable.
- Complete ongoing book task before any handoff.

For catalog browsing tips and genre descriptions, see [references/CATALOG_GUIDE.md](references/CATALOG_GUIDE.md). For a
sample book listing template, see [assets/book-listing-template.md](assets/book-listing-template.md).
