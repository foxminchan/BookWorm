@navigation
@p2
Feature: Dashboard Navigation
  As an admin
  I want to navigate between different sections
  So that I can efficiently manage the bookstore

  Background:
    Given the backoffice application is running
    And I am logged in as an admin

  @smoke
  @dashboard
  Scenario: View dashboard overview
    Given I am on the dashboard
    Then I should see the dashboard statistics
    And I should see the navigation menu

  @navigation
  Scenario: Navigate to books management
    Given I am on the dashboard
    When I click on "Books" in the navigation
    Then I should be on the books page
    And I should see "Books Management"

  @navigation
  Scenario: Navigate to categories management
    Given I am on the dashboard
    When I click on "Categories" in the navigation
    Then I should be on the categories page
    And I should see "Categories Management"

  @navigation
  Scenario: Navigate to authors management
    Given I am on the dashboard
    When I click on "Authors" in the navigation
    Then I should be on the authors page
    And I should see "Authors Management"

  @navigation
  Scenario: Navigate to publishers management
    Given I am on the dashboard
    When I click on "Publishers" in the navigation
    Then I should be on the publishers page
    And I should see "Publishers Management"

  @navigation
  Scenario: Navigate to orders management
    Given I am on the dashboard
    When I click on "Orders" in the navigation
    Then I should be on the orders page
    And I should see "Orders Management"

  @navigation
  Scenario: Navigate to customers management
    Given I am on the dashboard
    When I click on "Customers" in the navigation
    Then I should be on the customers page
    And I should see "Customers Management"

  @navigation
  Scenario: Navigate to reviews management
    Given I am on the dashboard
    When I click on "Reviews" in the navigation
    Then I should be on the reviews page
    And I should see "Reviews Management"

  @navigation
  @breadcrumb
  Scenario: Navigate back to dashboard from books page
    Given I am on the books page
    When I click on "Admin" in the breadcrumb
    Then I should be on the dashboard

  @mobile
  @responsive
  Scenario: Mobile devices should show mobile blocker
    Given I am using a mobile device
    When I access the backoffice
    Then I should see a mobile blocker message
    And I should not see the main content
