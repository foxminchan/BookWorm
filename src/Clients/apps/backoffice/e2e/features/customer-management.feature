@ordering @customers @p2
Feature: Customer Management
  As an admin
  I want to manage customer information
  So that I can provide better service and support

  Background:
    Given the backoffice application is running
    And I am logged in as an admin
    And I am on the customers page

  @smoke @view-customers
  Scenario: View list of customers
    When the customers page loads
    Then I should see the customers table
    And I should see customer information

  @view-customer-details
  Scenario: View customer details
    Given there is at least one customer
    When I click view on the first customer
    Then I should see the customer details page
    And I should see customer contact information
    And I should see customer order history

  @search @customers
  Scenario: Search for customers by name
    Given there are multiple customers
    When I search for a customer by name
    Then I should see only matching customers

  @search @customers
  Scenario: Search for customers by email
    Given there are multiple customers
    When I search for a customer by email
    Then I should see only matching customers

  @pagination @customers
  Scenario: Navigate through customer pages
    Given there are more than 10 customers
    When I navigate to page 2
    Then I should see different customers
    And the pagination should update
