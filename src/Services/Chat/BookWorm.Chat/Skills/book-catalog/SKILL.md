---
name: book-catalog
description: "Search and recommend books from BookWorm's catalog. Use when customers ask about finding books, getting personalized recommendations, exploring genres, discovering authors, comparing titles, checking book availability, filtering by price or rating, or looking up specific ISBNs."
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
3. **Rank results** — Prefer higher ratings, then popularity, then price.
4. **Provide context** — Include author, genre, a one-sentence description, and price.
5. **Suggest alternatives** — Offer 2–3 options when possible.

## Response Format

- Present books as a numbered or bulleted list.
- Include: **Title** — _Author_ — Genre — ⭐ Rating — 💲 Price.
- If no results, say so and offer to broaden the search.

## Handoff Rules

- Policy or account questions → hand off to QAAgent.
- Emotional feedback or complaints → hand off to SentimentAgent.
- Complete ongoing book task before any handoff.

For catalog browsing tips and genre descriptions, see [references/CATALOG_GUIDE.md](references/CATALOG_GUIDE.md).
For a sample book listing template, see [assets/book-listing-template.md](assets/book-listing-template.md).
