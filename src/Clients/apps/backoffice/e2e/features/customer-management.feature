@ordering
@customers
@p2
Feature: Customer Management
  As an admin
  I want to manage customer information
  So that I can maintain accurate customer records

  Background:
    Given the backoffice application is running
    And I am logged in as an admin
    And I am on the customers page

  @smoke
  @view-customers
  Scenario: View list of customers
    When the customers page loads
    Then I should see the customers table
    And I should see customer Name and Address columns

  @delete-customer
  Scenario: Delete a customer with confirmation
    Given there is at least one customer
    When I click the delete button for the first customer
    Then I should see a confirmation dialog
    When I confirm the customer deletion
    Then I should see a success message

  @delete-customer
  @cancel
  Scenario: Cancel customer deletion
    Given there is at least one customer
    When I click the delete button for the first customer
    And I cancel the customer deletion
    Then the customer should remain in the list

  @pagination
  @customers
  Scenario: Navigate through customer pages
    Given there are more than 10 customers
    When I navigate to the next page
    Then I should see different customers
    And the pagination should update
