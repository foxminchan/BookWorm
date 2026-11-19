@ordering
@orders
@p1
Feature: Order Management
  As an admin
  I want to manage customer orders
  So that I can track and fulfill orders efficiently

  Background:
    Given the backoffice application is running
    And I am logged in as an admin
    And I am on the orders page

  @smoke
  @view-orders
  Scenario: View list of orders
    When the orders page loads
    Then I should see the orders table
    And I should see order information including status and total

  @view-order-details
  Scenario: View order details
    Given there is at least one order
    When I click view on the first order
    Then I should see the order details page
    And I should see order summary cards
    And I should see ordered items
    And I should see the order event history

  @filter
  @orders
  Scenario: Filter orders by status - New
    Given there are orders with different statuses
    When I filter orders by "New" status
    Then I should see only "New" orders

  @filter
  @orders
  Scenario: Filter orders by status - Completed
    Given there are orders with different statuses
    When I filter orders by "Completed" status
    Then I should see only "Completed" orders

  @filter
  @orders
  Scenario: Filter orders by status - Cancelled
    Given there are orders with different statuses
    When I filter orders by "Cancelled" status
    Then I should see only "Cancelled" orders

  @update-order-status
  Scenario: Complete a New order
    Given there is at least one new order
    When I complete the first new order
    Then the order status should be updated
    And I should see a success message

  @update-order-status
  Scenario: Cancel a New order
    Given there is at least one new order
    When I cancel the first new order
    Then the order status should be updated
    And I should see a success message

  @pagination
  @orders
  Scenario: Navigate through order pages
    Given there are more than 10 orders
    When I navigate to the next page
    Then I should see different orders
    And the pagination should update
