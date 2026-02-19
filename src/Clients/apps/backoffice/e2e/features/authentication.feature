@critical
@authentication
@p0
Feature: Admin Authentication
  As an admin user
  I want to securely log in to the backoffice
  So that I can manage the bookstore

  Background:
    Given the backoffice application is running

  @smoke
  @login
  Scenario: Successful login redirects to dashboard
    Given I am on the login page
    When I enter valid admin credentials
    And I click the login button
    Then I should be redirected to the dashboard
    And I should see the navigation menu

  @smoke
  @login-redirect
  Scenario: Login redirects to Keycloak OAuth provider
    Given I am on the homepage
    When I navigate to the login page
    Then I should be redirected to Keycloak OAuth provider

  @validation
  @login-error
  Scenario: Login with invalid credentials shows error
    Given I am on the login page
    When I enter invalid admin credentials
    And I click the login button
    Then I should see an authentication error message
    And I should remain on the login page

  @protected-route
  Scenario: Accessing dashboard without authentication redirects to login
    Given I am not logged in
    When I navigate to the dashboard
    Then I should be redirected to the login page

  @protected-route
  Scenario: Accessing books management requires authentication
    Given I am not logged in
    When I navigate to "/catalog/books"
    Then I should be redirected to the login page

  @logout
  Scenario: Successful logout clears session
    Given I am logged in as an admin
    When I open the user menu
    And I click logout
    Then I should be redirected to the login page
    And my session should be cleared

  @session
  Scenario: Session persists across page navigation
    Given I am logged in as an admin
    When I navigate to the books page
    And I navigate to the orders page
    Then I should remain authenticated
    And I should see the navigation menu
