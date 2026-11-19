@critical
@authentication
@p0
Feature: User Authentication
  As a user
  I want to securely log in and register
  So that I can access my account and place orders

  Background:
    Given the storefront application is running

  @smoke
  @login
  Scenario: Successful login redirects to Microsoft Entra ID
    Given I am on the homepage
    When I open the account menu
    And I select "Login"
    Then I should be redirected to the login page
    And I should see "Redirecting to login..." message
    And I should be redirected to Microsoft Entra ID OAuth provider

  @smoke
  @register
  Scenario: Successful registration redirects to Microsoft Entra ID
    Given I am on the homepage
    When I open the account menu
    And I select "Register"
    Then I should be redirected to the registration page
    And I should see "Redirecting to registration..." message
    And I should be redirected to Microsoft Entra ID OAuth provider

  @navigation
  Scenario: Access login from navigation header
    Given I am on the homepage
    When I open the account menu
    Then I should see login options
    When I select "Login"
    Then I should be redirected to login

  @protected-route
  Scenario: Accessing protected route redirects to login
    Given I am not logged in
    When I navigate to "/account"
    Then I should be redirected to login page
    And the URL should contain "callbackUrl=%2Faccount"

  @protected-route
  Scenario: Accessing orders page requires authentication
    Given I am not logged in
    When I navigate to "/account/orders"
    Then I should be redirected to login page
    And the URL should contain "callbackUrl=%2Faccount%2Forders"

  @protected-route
  Scenario: Accessing basket page requires authentication
    Given I am not logged in
    Then the basket icon should not be visible in header
    When I navigate to "/basket"
    Then I should be redirected to login page
    And the URL should contain "callbackUrl=%2Fbasket"

  @logout
  Scenario: Successful logout clears session
    Given I am logged in as a customer
    When I open the account menu
    And I select "Logout"
    Then I should be logged out
    And I should be redirected to homepage
    And I should see "Login" option in account menu

  @session-persistence
  Scenario: User session persists across page navigation
    Given I am logged in as a customer
    When I navigate to the shop page
    Then I should still be logged in
    When I navigate to the basket page
    Then I should still be logged in
    And the account menu should show authenticated options

  @auth-redirect
  Scenario: Authenticated user visiting login redirects to homepage
    Given I am logged in as a customer
    When I navigate to "/login"
    Then I should be redirected to the homepage
