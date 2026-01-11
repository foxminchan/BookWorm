@navigation @p2
Feature: Dashboard Navigation
  As an admin
  I want to navigate between different sections
  So that I can efficiently manage the bookstore

  Background:
    Given the backoffice application is running
    And I am logged in as an admin

  @smoke @dashboard
  Scenario: View dashboard overview
    Given I am on the dashboard
    Then I should see the dashboard statistics
    And I should see the navigation menu

  @navigation
  Scenario: Navigate to books management
    Given I am on the dashboard
    When I click on "Catalog" in the navigation
    Then I should be on the books page

  @navigation
  Scenario: Navigate to orders management
    Given I am on the dashboard
    When I click on "Orders" in the navigation
    Then I should be on the orders page

  @navigation
  Scenario: Navigate to customers management
    Given I am on the dashboard
    When I click on "Customers" in the navigation
    Then I should be on the customers page

  @navigation
  Scenario: Navigate to reviews management
    Given I am on the dashboard
    When I click on "Reviews" in the navigation
    Then I should be on the reviews page

  @navigation @breadcrumb
  Scenario: Navigate back to dashboard from books page
    Given I am on the books page
    When I click on "Dashboard" in the breadcrumb
    Then I should be on the dashboard

  @mobile @responsive
  Scenario: Mobile devices should show mobile blocker
    Given I am using a mobile device
    When I access the backoffice
    Then I should see a mobile blocker message
    And I should not see the main content
