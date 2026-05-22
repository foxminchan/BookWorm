# Grilling Question Bank

Question templates for Phase 4 of the `code-to-catalog` skill. Tiered grilling: structural decisions only.

Every question follows the same shape:

1. **State the finding** with evidence (file path + line number).
2. **Give the recommended answer** with reasoning.
3. **Ask a single closed or nearly-closed question.**

The user confirms, corrects, or overrides. Move to the next question.

## Table of contents

- [Topic 1: Domains & boundaries](#topic-1-domains--boundaries)
- [Topic 2: Service boundaries](#topic-2-service-boundaries)
- [Topic 3: Message classification](#topic-3-message-classification)
- [Topic 4: Drift reconciliation](#topic-4-drift-reconciliation)
- [Handling disagreement](#handling-disagreement)
- [Things NOT to grill on](#things-not-to-grill-on)

## Topic 1: Domains & boundaries

### When multiple domain candidates exist

> I see three top-level groupings in the code: `src/orders/`, `src/payments/`, `src/shipping/`. My recommendation: treat these as three separate domains — **Orders**, **Payments**, **Shipping**. Services map to them by folder. Does that match how your team thinks about the boundaries?

### When only one candidate exists

> Everything in this codebase lives under a single top-level module. I'd recommend modeling it as a **single domain** for now — you can always split later. Does that work, or is there a domain split you have in mind?

### When a service could belong to two domains

> `NotificationService` has code dealing with order notifications (`src/notifications/orders.ts`) and payment notifications (`src/notifications/payments.ts`). It could live under **Orders**, under **Payments**, or in its own **Notifications** domain. My recommendation: own domain — it's cross-cutting and has its own lifecycle. Thoughts?

### When code structure and team structure might disagree

> The code groups by technical layer (`controllers/`, `services/`, `repositories/`), not by domain. If your team organizes by business capability (orders team, payments team, etc.), I'd use that instead of the folder structure. Which lens do you want the catalog to reflect — code structure or team structure?

### When existing catalog has domains that don't match code

> Your catalog has a **`Commerce`** domain that groups orders, payments, and shipping. The code is split into three separate top-level modules. Two options:
>
> 1. Keep `Commerce` as the single domain, put all three services under it.
> 2. Split into `Orders`, `Payments`, `Shipping` — this would be a restructure of the catalog.
>
> I'd lean toward option 1 — the catalog already reflects a business decision, and code layout doesn't have to match. Your call?

## Topic 2: Service boundaries

### Confirming a detected service

> I see a service at `services/orders/` — own `package.json`, own Dockerfile, entrypoint at `src/main.ts`. I'd call this **OrderService**. Confirm?

### When a package might or might not be a service

> There's a package at `packages/shared-events/` with type definitions only — no entrypoint, no Dockerfile. I'd treat it as **shared infrastructure**, not a service (these types become message payloads, not their own catalog resource). Agree?

### When handlers are split but deploy together

> The Lambda functions under `handlers/orders/` are separate files, but they deploy as one service via `serverless.yml`. I'd treat them as **one service** (`OrderService`) with multiple handlers. OK?

### When one codebase might actually be two services

> `services/api/` has both an HTTP API and a background worker in the same entrypoint. They could be one service or two. My recommendation: **one service with two roles** unless they have separate scaling/deployment needs. Do they deploy independently?

### When a service is in the catalog but not in the code

> Catalog has **`LegacyInventoryService`** (v1.2.0). I couldn't find it in this codebase. It might live in another repo, or it might be deprecated. I'll flag it as `investigate` in the plan — not delete. Sound right?

## Topic 3: Message classification

### Confident event (bulk confirmation)

> I identified these as **events** based on past-tense naming and pub/sub publication: `OrderPlaced`, `OrderCancelled`, `PaymentProcessed`, `UserRegistered`. Any you'd reclassify?

### Confident command (bulk confirmation)

> I identified these as **commands** based on imperative naming and single-handler delivery: `PlaceOrder`, `CancelOrder`, `ProcessPayment`, `SendEmail`. Any you'd reclassify?

### Confident query (bulk confirmation)

> These look like **queries** — read-only naming, synchronous response expected: `GetOrder`, `FindUser`, `ListPayments`. Confirm?

### Uncertain — ambiguous name

> `OrderProcessed` could be an **event** (post-processing notification) or a **command** (do the processing). The code at `src/orders/handlers.ts:42` shows it being consumed from a queue and no reply is sent — I'd call it an **event**. Agree, or is this actually a command?

### Uncertain — no producer found

> I see a type definition for `InventoryReserved` at `src/types/inventory.ts:12` but I can't find anywhere it's emitted. My guess: **event**, emitted by a service outside this codebase. Is that right, or is this legacy/unused?

### Uncertain — conflicting indicators

> `PaymentAuthorization` has a past-tense vibe but is sent to a single handler that returns a result. That's more command-like. I'd classify it as a **command**. Agree?

### When the framework is explicit

> Your codebase uses MediatR — `INotification` for events, `IRequest<T>` for queries/commands. I'll trust the interface classification directly. Any messages you know are misclassified in the code?

## Topic 4: Drift reconciliation

### New send detected

> Catalog says `PaymentService` sends `[PaymentProcessed]`. Code at `src/payments/refund.ts:24` also emits `PaymentRefunded`. I'd add `PaymentRefunded` to the service's sends and flag the message as `new`. OK?

### New receive detected

> Catalog says `OrderService` receives `[PlaceOrder]`. Code at `src/orders/handlers.ts:88` also consumes `CancelOrder`. Add `CancelOrder` to its receives?

### Schema drift

> Catalog's `OrderPlaced` schema has a field `amount_cents` (snake_case). Code has `amountCents` (camelCase) at `src/orders/types.ts:15`. Two possibilities:
>
> 1. Code was renamed and catalog is stale → update catalog.
> 2. Catalog is the official contract and code is wrong → flag for the team.
>
> Which is it?

### Resource in catalog, not in code

> Catalog has `LegacyPaymentConfirmed` (event, v0.0.3). Not found anywhere in this codebase. I'll flag it as `investigate` — do not delete. You can decide later whether it's deprecated or lives in another repo. Sound good?

### Version question

> `PaymentService` in the catalog is v1.2.0. The code has added a new event (`PaymentRefunded`). Should this bump to v1.3.0 (minor — additive) or keep as v1.2.0 and let the user decide during `catalog-documentation-creator`? I'd defer the version bump to the next skill. OK?

## Handling disagreement

When the user disagrees with your recommendation:

- **Accept it without arguing.** The user knows their system. Your job is to capture their answer, not debate.
- **Capture the reasoning** if they give it, so it lands in the plan's "Open decisions / rationale" section.
- **Check for downstream impact.** If they reclassify something, does it change other decisions? Surface that: "Since you're reclassifying `OrderProcessed` as a command, it'll now go to the service's receives instead of sends. OK?"

When the user's answer seems to contradict the evidence:

- **Surface the conflict once**, politely: "Got it — I'll mark it as a command. For reference, the code at `src/orders/handlers.ts:42` looks more like an event handler. Worth a quick look later?"
- Do not keep pressing. One flag is enough.

When the user doesn't know:

- Offer to **defer** it: "We can leave this as `uncertain` in the plan and decide later. Move on?"
- If that happens more than 2–3 times, suggest pausing: "We can save what we have to the plan file and come back when you've talked to the team. Sound good?"

## Things NOT to grill on

These pass through to `catalog-documentation-creator` with defaults. Do **not** ask the user about them during `code-to-catalog`:

- **Summary text** for each resource — the next skill writes these.
- **Owners / teams** — unless they're already in `CODEOWNERS` and ambiguous. If ambiguous, one quick question is fine; otherwise leave it for the next skill.
- **Schema details** — field types, nullability, descriptions. That's schema-authoring work, not architectural work.
- **Badges, visual styling** — not structural.
- **Flow diagrams** — that's `flow-wizard`.
- **Version numbers** — default to `0.0.1` for new, defer to the next skill for updates.
- **Folder structure preferences** (nested vs flat) — `catalog-documentation-creator` infers from the existing catalog.

If you find yourself asking about any of the above, stop and move on to the next structural topic.
