---
name: flow-wizard
description: Guides users through documenting business flows step-by-step in EventCatalog. Use when user asks to "document a flow", "map a business process", "create a flow diagram", "walk through a process", "document an end-to-end flow", or "map out how something works in my architecture".
license: MIT
metadata:
  author: eventcatalog
  version: "1.0.0"
---

# Flow Documentation Wizard

An interactive, conversational skill that guides users through documenting business flows in EventCatalog — step by step, section by section — while cross-referencing their existing catalog resources.

## Instructions

### Step 1: Locate the User's Catalog

Before anything else, you need to find the user's EventCatalog project so you can cross-reference existing resources.

Ask: **"Do you have an EventCatalog project I can look at? If so, where is it?"**

- If they provide a path, verify it's an EventCatalog project by checking for `eventcatalog.config.js` or known directories (`services/`, `events/`, `domains/`, `flows/`).
- If they have the EventCatalog MCP server connected, use `getResources` to discover what's in the catalog.
- If they don't have one, that's fine — you'll document steps as plain text descriptions without resource references. Let them know they can still create a useful flow and add resource links later.

Once located, scan the catalog to build an inventory of existing resources:

- Read existing services, events, commands, queries, domains, channels, and flows
- Note their IDs, names, and versions — you'll use these to suggest matches as the user walks through their flow

### Step 2: Ask What Flow They Want to Document

Ask: **"What flow would you like to document? Describe the end-to-end process you're trying to capture."**

Let the user describe the flow in their own words. Examples:

- "User signs up, gets a welcome email, and is added to our CRM"
- "Order is placed, payment is processed, inventory is reserved, and the order is shipped"
- "A customer submits a support ticket, it gets triaged, assigned to an agent, and resolved"

From their description, break the flow into **sections** (high-level stages). Present these sections back to the user for confirmation:

**Example response:**

> Based on your description, I can see roughly these sections:
>
> 1. **User Registration** — the user signs up
> 2. **Welcome Communication** — welcome email is sent
> 3. **CRM Integration** — user is added to the CRM
>
> Does that look right? We'll walk through each one in detail. Feel free to add, remove, or reorder sections.

Wait for confirmation before proceeding.

### Step 3: Walk Through Each Section

Go through each section **one at a time**. For each section, ask the user to describe what happens. The user leads the conversation — you follow.

For each section, ask something like:

**"Let's start with [Section Name]. What happens here? Who or what triggers it, and what happens as a result?"**

As the user describes what happens, you need to:

#### A. Identify the Step Type

From their description, determine which flow step type(s) this section involves:

- **Actor** — a person or role does something (e.g., "the user clicks sign up", "the admin approves")
- **Service** — a service in their architecture processes something (e.g., "our auth service handles registration")
- **Message** — an event, command, or query is exchanged (e.g., "a UserRegistered event is fired")
- **External System** — a third-party system is involved (e.g., "we call Stripe for payment", "Twilio sends the SMS")

A single section may produce multiple steps (e.g., a user action triggers a command, which is handled by a service, which emits an event).

#### B. Cross-Reference the Catalog

For each step the user describes, search their EventCatalog (if available) for matching resources:

1. **Services** — Does the service they mention exist in the catalog? Search by name, ID, or similar terms.
2. **Events/Commands/Queries** — Does the message they describe match something already documented? Look for exact matches and close matches (e.g., user says "order created event" → search for `OrderCreated`, `OrderPlaced`, etc.).
3. **Channels** — Are there channels relevant to how this message flows?
4. **Domains** — Does this step fall within a documented domain?

**If you find a match**, tell the user:

> I found a **[resource type]** called `[ResourceName]` (version `[version]`) in your catalog that looks like it matches. Would you like to reference it in this step?

If they say yes, use the resource's actual `id` and `version` in the flow step.

**If you find multiple possible matches**, present them and let the user choose:

> I found a few resources that might match what you're describing:
>
> - `OrderCreated` (event, v1.0.0) — "Emitted when a new order is placed"
> - `OrderPlaced` (event, v0.0.1) — "Published when checkout completes"
>
> Which one fits, or is this something new?

**If you find no match**, that's perfectly fine. Document it as a descriptive step:

- If it sounds like a service, use `service` type with a suggested ID and note it's not yet in the catalog
- If it sounds like a message, use `message` type with a suggested ID
- If it's a person/role, use `actor` type
- If it's a third-party tool/API, use `externalSystem` type

Let the user know: **"I didn't find this in your catalog. I'll document it as [step type] for now. You can create the full resource later if you'd like."**

#### C. Confirm Each Section Before Moving On

After processing a section, summarize what you've captured for that section and ask the user to confirm before moving to the next one.

**Example:**

> Here's what I've captured for **Payment Processing**:
>
> 1. `ProcessPayment` command is sent (matched to your existing `ProcessPayment` command v0.0.1)
> 2. `PaymentService` handles the payment (matched to your existing `PaymentService` v1.2.0)
> 3. Calls **Stripe** (external system) to charge the card
> 4. `PaymentProcessed` event is emitted (not in your catalog yet — I'll use a placeholder)
>
> Does that look right? Anything to add or change?

### Step 4: Handle Branching and Edge Cases

As you walk through sections, watch for:

- **Branching paths** — "If payment succeeds, we continue; if it fails, we notify the user"
  - Ask: **"It sounds like there's a decision point here. What are the possible paths?"**
  - Document these as `next_steps` (plural) with labels for each branch

- **Loops** — "The service retries if the external call fails"
  - Document as a step that references back to an earlier step via `next_step`

- **Error/failure paths** — "If validation fails, we send back an error"
  - These are terminal steps or branch to a notification/error handling step

- **Parallel paths** — "While payment processes, we also reserve inventory"
  - Document as separate branches from a common step using `next_steps`

When you detect branching, explicitly confirm the paths with the user:

> **"It sounds like there's a fork here. After [step], these things could happen:**
>
> 1. [Success path] → continues to [next section]
> 2. [Failure path] → [what happens]
>
> **Is that right? Are there other outcomes?"**

### Step 5: Build the Flow Summary

After walking through all sections, present a complete summary of the flow before generating the file:

> **Here's the complete flow: [Flow Name]**
>
> 1. [Step 1 summary] → [Step 2]
> 2. [Step 2 summary] → [Step 3]
> 3. [Step 3 summary] → branches to [Step 4a] or [Step 4b]
>    ...
>
> **Resources matched from your catalog:** [list of matched resources]
> **New resources (not yet in catalog):** [list of unmatched items]
>
> Ready to generate the flow file?

### Step 6: Generate the Flow File

Once the user confirms, generate the flow `index.mdx` file following the format in `references/flows.md`.

**Rules for generation:**

1. Use the user's catalog directory. Ask where the flow should be saved if unclear — either `flows/{FlowName}/index.mdx` or `domains/{Domain}/flows/{FlowName}/index.mdx` depending on their catalog structure.

2. For **matched resources** (found in catalog), use the exact `id` and `version` from the catalog:

   ```yaml
   - id: "payment_processing"
     title: "Payment Service"
     service:
       id: "PaymentService"
       version: "1.2.0"
   ```

3. For **unmatched resources** (not in catalog), use descriptive IDs and version `0.0.1`:

   ```yaml
   - id: "send_welcome_email"
     title: "Send Welcome Email"
     message:
       id: "WelcomeEmailRequested"
       version: "0.0.1"
   ```

4. For **actors**, use the role or persona name:

   ```yaml
   - id: "user_initiates_signup"
     title: "User Signs Up"
     actor:
       name: "Customer"
   ```

5. For **external systems**, include name, summary, and URL if known:

   ```yaml
   - id: "stripe_charges_card"
     title: "Stripe Payment"
     externalSystem:
       name: "Stripe"
       summary: "Third-party payment processor"
       url: "https://stripe.com"
   ```

6. Connect all steps with `next_step` or `next_steps` as appropriate.

7. Terminal steps (end of flow or end of a branch) should have no `next_step`.

8. The body should be `<NodeGraph />`.

9. Set `version` to `0.0.1` for new flows.

10. Write a meaningful `summary` that describes the end-to-end business process.

### Step 7: Validate and Write

Before writing the file:

1. Verify all step IDs are unique within the flow
2. Verify all `next_step` / `next_steps` references point to valid step IDs
3. Verify matched resource IDs and versions are correct
4. Verify the YAML frontmatter is valid
5. Verify `<NodeGraph />` is in the body

Write the file to the user's catalog directory.

After writing, let the user know:

- Where the file was saved
- Which resources were matched from their catalog
- Which resources are new/unmatched — suggest they can document these later using the `catalog-documentation-creator` skill
- They can view the flow visualization in EventCatalog by running their dev server

## Conversation Guidelines

- **The user leads.** You ask questions and guide, but the user describes what happens in their own words. Don't assume or invent steps.
- **One section at a time.** Don't rush ahead. Confirm each section before moving on.
- **Be helpful with matches.** When you find catalog resources that match, proactively suggest them — but always let the user decide.
- **Be honest about misses.** If nothing in the catalog matches, say so plainly and document it descriptively.
- **Keep it conversational.** This is a guided walkthrough, not a form to fill out. Be natural and responsive.
- **Suggest, don't dictate.** If the user's description doesn't perfectly map to a step type, suggest what you think fits and ask if that's right.
- **Handle complexity gracefully.** If a flow gets complex (many branches, loops), help the user keep track by summarizing periodically.

## Searching the Catalog

When searching for matching resources, use these strategies:

1. **Exact ID match** — search for the exact name mentioned (e.g., "OrdersService" → look for `OrdersService`)
2. **Fuzzy match** — search for variations (e.g., "orders service" → try `OrdersService`, `OrderService`, `orders-service`)
3. **Semantic match** — if the user says "the thing that handles payments", search for services with "payment" in the name or summary
4. **Browse by type** — if you know it's an event, search events specifically

If the EventCatalog MCP server is available:

- Use `getResources` to list all resources
- Use `getResource` to get details on a specific resource
- Use `findResourcesByOwner` to explore resources by team

If reading the filesystem directly:

- Look in `services/`, `events/`, `commands/`, `queries/`, `domains/`, `flows/`, `channels/` directories
- Read `index.mdx` files to check `id`, `name`, `version`, and `summary` fields

## Quality Checklist

Before delivering the flow file:

1. Every step has a unique `id`
2. Every step has a `title`
3. Every step has exactly one type (`actor`, `service`, `message`, or `externalSystem`)
4. All `next_step` and `next_steps` references point to valid step IDs within the flow
5. No orphaned steps (every step is reachable from the first step, except via branching)
6. Matched resources use correct `id` and `version` from the catalog
7. The flow has a clear start (first step) and at least one terminal step (no `next_step`)
8. YAML frontmatter is valid with `---` delimiters
9. File is named `index.mdx`
10. `<NodeGraph />` is in the body
11. `version` is a valid semver string
12. `summary` describes the business process meaningfully
