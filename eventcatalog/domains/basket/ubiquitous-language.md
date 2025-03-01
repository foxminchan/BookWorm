---
dictionary:
  - id: Basket
    name: Basket
    summary: "A temporary container that holds enchanted items a customer intends to purchase."
    description: |
      A basket represents a customer's current selection of items before proceeding to checkout. Key aspects include:

      - Items added by the customer
      - Quantity of each item
      - Subtotal calculation
      - Expiration or timeout period for inactive baskets

      Baskets are essential for managing the pre-purchase phase and ensuring a smooth transition to the checkout process.
    icon: ShoppingCart

  - id: Basket Item
    name: Basket Item
    summary: "An individual enchanted product or service added to the basket by the customer."
    description: |
      Each basket item represents a specific product or service selected by the customer. It includes:

      - Product identifier (SKU or part number)
      - Quantity selected
      - Price per unit
      - Total price for the item

      Basket items are used to calculate the total cost and manage inventory reservations.
    icon: List

  - id: Basket Expiry
    name: Basket Expiry
    summary: "The time limit after which an inactive basket is automatically cleared."
    description: |
      Basket expiry ensures that abandoned baskets do not hold inventory indefinitely. Key features include:

      - Configurable timeout period (e.g., 30 minutes)
      - Automatic removal of items after expiry
      - Notification to the customer before expiry

      This mechanism helps free up inventory for other customers and maintains system efficiency.
    icon: Clock

  - id: Basket Subtotal
    name: Basket Subtotal
    summary: "The total cost of all items in the basket before taxes, discounts, or shipping fees."
    description: |
      The basket subtotal is calculated by summing the total price of all basket items. It includes:

      - Individual item prices multiplied by quantities
      - Adjustments for discounts or promotions
      - Displayed prominently to the customer

      The subtotal is a key component of the checkout process and helps customers understand their total cost.
    icon: Calculator

  - id: Basket Discount
    name: Basket Discount
    summary: "A reduction in the total cost of the basket, applied through promotions or special offers."
    description: |
      Basket discounts are applied to the subtotal and can be triggered by:

      - Promo codes or coupons
      - Automatic discounts based on basket value
      - Special offers for specific items or categories

      Discounts enhance customer satisfaction and encourage purchases.
    icon: Tag

  - id: Basket Merge
    name: Basket Merge
    summary: "The process of combining items from multiple baskets into a single basket."
    description: |
      Basket merging occurs when a customer logs in or switches devices, and their baskets are consolidated. It involves:

      - Identifying duplicate items
      - Summing quantities for matching items
      - Preserving the most recent additions

      This ensures a seamless shopping experience across devices or sessions.
    icon: Merge

  - id: Basket Reservation
    name: Basket Reservation
    summary: "A temporary hold placed on inventory items added to a basket."
    description: |
      Basket reservations prevent overselling by ensuring that items in a basket are not available to other customers. Key aspects include:

      - Duration of the reservation (e.g., until checkout or expiry)
      - Automatic release of reserved items if the basket expires
      - Real-time inventory updates

      Reservations are critical for maintaining accurate stock levels.
    icon: Lock

  - id: Basket Checkout
    name: Basket Checkout
    summary: "The process of finalizing the purchase of items in the basket."
    description: |
      Basket checkout transitions the customer from selecting items to completing their purchase. It includes:

      - Reviewing basket contents
      - Applying discounts or promotions
      - Calculating taxes and shipping fees
      - Collecting payment information

      A smooth checkout process is vital for customer satisfaction and conversion rates.
    icon: CreditCard

  - id: Basket Abandonment
    name: Basket Abandonment
    summary: "When a customer leaves their basket without completing the purchase."
    description: |
      Basket abandonment is a key metric for understanding customer behavior. It involves:

      - Tracking baskets that reach the checkout stage but are not completed
      - Analyzing reasons for abandonment (e.g., high shipping costs, complex checkout)
      - Implementing strategies to recover abandoned baskets (e.g., reminder emails)

      Reducing abandonment rates improves overall sales performance.
    icon: AlertCircle

  - id: Basket Recovery
    name: Basket Recovery
    summary: "The process of encouraging customers to return and complete their abandoned baskets."
    description: |
      Basket recovery strategies aim to convert abandoned baskets into completed purchases. Common methods include:

      - Sending reminder emails with a link to the basket
      - Offering discounts or incentives to complete the purchase
      - Simplifying the checkout process

      Effective recovery efforts can significantly boost revenue.
    icon: RefreshCw
---
