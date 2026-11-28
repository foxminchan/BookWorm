# Review & Rating Analysis Guide

## Deterministic Quality Classification (Application Reference)

The Rating service computes the final `RatingClassification` deterministically in code (`RatingClassifier`) from `AverageRating`, `TotalReviews`, and `DominantNegativePattern`:

| Classification | Criteria                                                                                               |
| -------------- | ------------------------------------------------------------------------------------------------------ |
| `No Data`      | `ReviewCount < 3`, or `AverageRating` is non-finite / outside `0.0–5.0`                                |
| `Bad`          | `AverageRating < 3.5`, OR `DominantNegativePattern == true`                                            |
| `Top Rated`    | (`AverageRating >= 4.5` AND `ReviewCount >= 50`) OR (`AverageRating >= 4.0` AND `ReviewCount >= 200`)  |
| `Good`         | All other cases with `ReviewCount >= 3` and `AverageRating >= 3.5` without a dominant negative pattern |

> **Important**: The AI agent must not invent or override this classification. Its role is to summarize reviews accurately and assess `DominantNegativePattern` with checkable evidence and confidence.

## Handling Edge Cases

- **Zero reviews**: Report clearly that no customer reviews are available for the book and state that quality signals cannot be determined.
- **Few reviews (1–2 reviews)**: Summarize the available comments while explicitly noting in **Data limitations** that the sample is too small to be representative.
- **Rating vs. text mismatch**: If the numeric `AverageRating` is high (e.g., 4.5+) but recent or detailed reviews raise recurring complaints (e.g., printing defects, outdated edition, misleading description), highlight the conflict and set `Confidence` to `Medium` or `Low`.
- **Large review sets**: Distinguish recurring themes (mentioned across multiple reviews) from isolated one-off remarks, and account for unequal batch sizes when reducing partial summaries.
