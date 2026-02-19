@critical
@checkout
@p0
Feature: Complete Purchase Journey
  As a customer
  I want to browse, add items to basket, and complete checkout
  So that I can purchase books

  Background:
    Given the storefront application is running
    And the catalog has available books

  @smoke
  Scenario: Successful order placement from empty state to confirmation
    Given I am on the homepage
    When I click "Browse Collection"
    Then I should see the shop page with books displayed

    When I click on the first book
    Then I should see the product detail page
    And the book details should be visible

    When I increase quantity to 2
    And I click "Add to Basket"
    Then the basket icon should show "2" items

    When I click the basket icon
    Then I should see the basket page with 2 items
    And the subtotal should be calculated correctly
    And the total should include "$5.00" shipping

    When I click "Checkout"
    Then I should be redirected to the confirmation page
    And I should see a success message with order ID
    And I should see the order total
    And the basket icon should show "0" items

  Scenario: Multi-item basket management before checkout
    Given I have added 3 different books to my basket
    When I go to the basket page
    Then I should see all 3 items

    When I increase quantity of item 1 to 3
    And I decrease quantity of item 2 to 0
    And I click "Save Changes"
    Then I should see a removal confirmation dialog for item 2

    When I confirm the removal
    Then item 2 should be removed from the basket
    And I should see 2 items in the basket
    And the total should be recalculated

  Scenario: Empty basket prevents checkout
    Given I am on the basket page
    And my basket is empty
    Then I should see "Your basket is empty" message
    And I should see "Explore the Collection" link
    And there should be no checkout button visible

  @edge-case
  Scenario: Basket persists across page navigation
    Given I have added 2 books to my basket
    When I navigate to the homepage
    Then the basket icon should show "2" items

    When I navigate to the shop page
    Then the basket icon should show "2" items

    When I click the basket icon
    Then I should see 2 items in the basket

  @quantity
  Scenario: Update multiple item quantities and verify total
    Given I have added a book priced at "$29.99" to my basket
    And I have added a book priced at "$49.99" to my basket
    When I go to the basket page
    And I set quantity of item 1 to 3
    And I set quantity of item 2 to 2
    And I click "Save Changes"
    Then the subtotal should be "$189.95"
    And the shipping should be "$5.00"
    And the total should be "$194.95"

  @validation
  Scenario: Zero quantity triggers removal confirmation
    Given I have added 1 book to my basket
    When I go to the basket page
    And I set quantity of item 1 to 0
    And I click "Save Changes"
    Then I should see a removal confirmation dialog

    When I cancel the removal
    Then the item should remain in the basket
    And the quantity should be restored to 1

  @clear-basket
  Scenario: Clear entire basket
    Given I have added 5 books to my basket
    When I go to the basket page
    And I click "Clear Basket"
    Then my basket should be empty
    And I should see "Your basket is empty" message
