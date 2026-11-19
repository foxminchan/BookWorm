@high-priority
@orders
Feature: Order Management
  As a customer
  I want to view and track my orders
  So that I can see my purchase history and order details

  Background:
    Given the storefront application is running
    And I am logged in as a customer

  @smoke
  Scenario: View order history
    Given I have placed multiple orders
    When I navigate to my orders page
    Then I should see a list of my orders
    And each order should display order ID, date, status, and total

  @smoke
  Scenario: View order details
    Given I have an order in my history
    When I click on an order from the list
    Then I should see the complete order details
    And I should see all ordered items with quantities and prices
    And I should see subtotal, shipping, and total amounts

  Scenario: Filter orders by status
    Given I have orders with different statuses
    When I filter orders by "Completed"
    Then I should only see completed orders

  Scenario: View order from confirmation page
    Given I have just completed a purchase
    When I click "View Order" on confirmation page
    Then I should be redirected to the order detail page
    And the order status should be "New"

  Scenario: View empty order history
    Given I am a new customer with no orders
    When I navigate to my orders page
    Then I should see "No Orders Found" message

  @pagination
  Scenario: Paginate through orders
    Given I have more than 5 orders
    When I view my order history
    And I click page 2 of orders
    Then I should see the next page of orders

  Scenario: Navigate back from order details
    Given I am viewing an order detail page
    When I click "Back to Orders"
    Then I should return to my order history page

  @account-integration
  Scenario: Access orders from account page
    Given I am on my account page
    When I click the "Order History" link on account page
    Then I should be redirected to the orders page
