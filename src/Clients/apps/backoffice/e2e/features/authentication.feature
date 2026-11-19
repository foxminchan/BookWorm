@critical
@authentication
@p0
Feature: Admin Authentication
  As an admin user
  I want to securely authenticate via Microsoft Entra ID
  So that only authorized admins can manage the bookstore

  Background:
    Given the backoffice application is running

  @smoke
  @login
  Scenario: Authenticated admin sees the dashboard
    Given I am logged in as an admin
    When I navigate to the dashboard
    Then I should be on the dashboard
    And I should see the navigation menu
    And I should see "Admin Portal"

  @protected-route
  Scenario: Accessing dashboard without authentication triggers Entra ID OAuth
    Given I am not logged in
    When I navigate to the dashboard
    Then I should see the authentication required prompt
    And Microsoft Entra ID OAuth sign-in should be triggered

  @protected-route
  Scenario: Accessing books management without authentication triggers Entra ID OAuth
    Given I am not logged in
    When I navigate to "/books"
    Then I should see the authentication required prompt
    And Microsoft Entra ID OAuth sign-in should be triggered

  @logout
  Scenario: Successful logout via user menu clears session
    Given I am logged in as an admin
    And I am on the dashboard
    When I open the user menu
    And I click log out in the user menu
    Then my session should be cleared
    And I should see the authentication required prompt

  @logout
  Scenario: Successful logout via sidebar button clears session
    Given I am logged in as an admin
    And I am on the dashboard
    When I click the sidebar logout button
    Then my session should be cleared
    And I should see the authentication required prompt

  @session
  Scenario: Session persists across page navigation
    Given I am logged in as an admin
    When I navigate to the books page
    And I navigate to the orders page
    Then I should remain authenticated
    And I should see the navigation menu
