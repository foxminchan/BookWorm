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
    And I should see customer information
    And I should see ordered items

  @search
  @orders
  Scenario: Search for orders by order number
    Given there are multiple orders
    When I search for an order by order number
    Then I should see only matching orders

  @filter
  @orders
  Scenario: Filter orders by status
    Given there are orders with different statuses
    When I filter orders by "Pending" status
    Then I should see only pending orders

  @filter
  @orders
  Scenario: Filter orders by status - Processing
    Given there are orders with different statuses
    When I filter orders by "Processing" status
    Then I should see only processing orders

  @filter
  @orders
  Scenario: Filter orders by status - Completed
    Given there are orders with different statuses
    When I filter orders by "Completed" status
    Then I should see only completed orders

  @update-order-status
  Scenario: Update order status from Pending to Processing
    Given there is at least one pending order
    When I change the order status to "Processing"
    Then the order status should be updated
    And I should see a success message

  @update-order-status
  Scenario: Update order status from Processing to Shipped
    Given there is at least one processing order
    When I change the order status to "Shipped"
    Then the order status should be updated
    And I should see a success message

  @pagination
  @orders
  Scenario: Navigate through order pages
    Given there are more than 10 orders
    When I navigate to page 2
    Then I should see different orders
    And the pagination should update
