@critical @authentication @p0
Feature: User Authentication
  As a user
  I want to securely log in and register
  So that I can access my account and place orders

  Background:
    Given the storefront application is running

  @smoke @login
  Scenario: Successful login redirects to Keycloak
    Given I am on the homepage
    When I click "Login"
    Then I should be redirected to the login page
    And I should see "Redirecting to login" message
    And I should be redirected to Keycloak OAuth provider

  @smoke @register
  Scenario: Successful registration redirects to Keycloak
    Given I am on the homepage
    When I click "Register"
    Then I should be redirected to the registration page
    And I should see "Redirecting to registration" message
    And I should be redirected to Keycloak OAuth provider

  @navigation
  Scenario: Access login from navigation header
    Given I am on the homepage
    When I click the user icon in header
    Then I should see login options
    When I select "Sign In"
    Then I should be redirected to login

  @protected-route
  Scenario: Accessing protected route redirects to login
    Given I am not logged in
    When I navigate to "/account"
    Then I should be redirected to login page

  @protected-route
  Scenario: Accessing orders page requires authentication
    Given I am not logged in
    When I navigate to "/account/orders"
    Then I should be redirected to login page

  @logout
  Scenario: Successful logout clears session
    Given I am logged in as a customer
    When I click the user menu
    And I select "Logout"
    Then I should be logged out
    And I should be redirected to homepage
    And I should see "Login" button in header

  @session-persistence
  Scenario: User session persists across page navigation
    Given I am logged in as a customer
    When I navigate to the shop page
    Then I should still be logged in
    When I navigate to the basket page
    Then I should still be logged in
    And the user menu should show my account name

  @checkout-auth
  Scenario: Checkout requires authentication
    Given I am not logged in
    And I have items in my basket
    When I attempt to checkout
    Then I should be redirected to login page
    And after login I should return to checkout

  @token-expiry
  Scenario: Expired token prompts re-authentication
    Given I am logged in as a customer
    And my authentication token has expired
    When I navigate to my account page
    Then I should be prompted to login again

  @oauth-callback
  Scenario: OAuth callback handles success
    Given I initiated OAuth login
    When the OAuth provider returns success
    Then I should be redirected to the homepage
    And I should be logged in

  @oauth-callback
  Scenario: OAuth callback handles error
    Given I initiated OAuth login
    When the OAuth provider returns an error
    Then I should see an error message
    And I should remain on the login page
