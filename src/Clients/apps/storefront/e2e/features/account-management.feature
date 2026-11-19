@high-priority
@account
@user-profile
Feature: User Account Management
  As a customer
  I want to view my profile, update my delivery address, and access my order
  history
  So that I can keep my delivery details up-to-date

  Background:
    Given the storefront application is running
    And I am logged in as a customer

  @smoke
  @profile
  Scenario: View account dashboard
    Given I am on the homepage
    When I navigate to my account page
    Then I should see my account dashboard
    And I should see my profile information
    And I should see my delivery address section
    And I should see the "Order History" link
    And I should see the account logout button

  @addresses
  @smoke
  Scenario: Update delivery address
    Given I am on my account page
    When I click "Edit" on the delivery address section
    And I fill in the delivery address form:
      | Field    | Value       |
      | Street   | 123 Main St |
      | City     | New York    |
      | Province | NY          |
    And I click "Save Address"
    Then my delivery address should display "123 Main St"

  @addresses
  Scenario: Cancel editing delivery address
    Given I am on my account page
    When I click "Edit" on the delivery address section
    And I fill in the delivery address form:
      | Field    | Value        |
      | Street   | 999 Temp Ave |
      | City     | Boston       |
      | Province | MA           |
    And I click "Cancel"
    Then the delivery address form should close

  @validation
  Scenario: Delivery address form validates required fields
    Given I am on my account page
    When I click "Edit" on the delivery address section
    And I clear the delivery address fields
    And I click "Save Address"
    Then the delivery address form should remain open

  @navigation
  Scenario: Navigate to order history from account page
    Given I am on my account page
    When I click the "Order History" link on account page
    Then I should be redirected to "/account/orders"

  @logout
  Scenario: Logout from account page
    Given I am on my account page
    When I click the account logout button
    Then I should be logged out
    And I should be redirected to homepage
