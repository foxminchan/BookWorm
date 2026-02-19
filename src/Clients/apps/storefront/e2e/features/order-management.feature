@high-priority
@orders
Feature: Order Management
  As a customer
  I want to view and manage my orders
  So that I can track my purchases and order history

  Background:
    Given the storefront application is running
    And I am logged in as a customer

  @smoke
  Scenario: View order history
    Given I have placed multiple orders
    When I navigate to "My Orders"
    Then I should see a list of my orders
    And each order should display order ID, date, status, and total

  @smoke
  Scenario: View order details
    Given I have an order in my history
    When I click on an order from the list
    Then I should see the complete order details
    And I should see buyer information
    And I should see delivery address
    And I should see all ordered items with quantities and prices
    And I should see subtotal, shipping, and total amounts

  Scenario: Filter orders by status
    Given I have orders with different statuses
    When I filter orders by "Completed"
    Then I should only see completed orders

  Scenario: Search for specific order
    Given I have multiple orders
    When I search for an order by its ID
    Then I should see the matching order
    And other orders should not be displayed

  Scenario: View order from confirmation page
    Given I have just completed a purchase
    When I click "View Order Details" on confirmation page
    Then I should be redirected to the order detail page
    And the order status should be "New"

  @critical
  Scenario: Cancel a new order
    Given I have a new order that hasn't been processed
    When I click "Cancel Order"
    And I confirm the cancellation
    Then the order status should change to "Cancelled"
    And I should see a cancellation confirmation message

  Scenario: Cannot cancel completed order
    Given I have a completed order
    When I view the order details
    Then the "Cancel Order" button should not be visible

  Scenario: Reorder from order history
    Given I have a completed order
    When I click "Reorder"
    Then all items from that order should be added to my basket
    And I should be redirected to the basket page

  Scenario: Download order invoice
    Given I have a completed order
    When I click "Download Invoice"
    Then an invoice PDF should be downloaded

  Scenario: View empty order history
    Given I am a new customer with no orders
    When I navigate to "My Orders"
    Then I should see "No orders found" message
    And I should see a "Start Shopping" button

  Scenario: Sort orders by date
    Given I have multiple orders
    When I sort orders by "Newest First"
    Then orders should be displayed with most recent first

  Scenario: View order tracking information
    Given I have an order that is being shipped
    When I view the order details
    Then I should see tracking information
    And I should see the tracking number

  @pagination
  Scenario: Load more orders
    Given I have more than 10 orders
    When I view my order history
    And I click "Load More"
    Then I should see additional orders loaded

  Scenario: Navigate back from order details
    Given I am viewing an order detail page
    When I click "Back to Orders"
    Then I should return to my order history page

  Scenario: View order item details from order
    Given I am viewing an order detail page
    When I click on an item in the order
    Then I should be redirected to that product's detail page
    And I can add it to my basket again

  @account-integration
  Scenario: Access orders from account page
    Given I am on the account page
    When I click "My Orders" link
    Then I should be redirected to the orders page

  @mobile-experience
  Scenario: Order management on mobile devices
    Given I am on a mobile device
    And I am on the orders page
    Then the order list should be mobile-friendly
    And I can swipe to see order details
    And all actions should be accessible
    Then I should be redirected to that book's detail page
