@high-priority @account @user-profile
Feature: User Account Management
  As a customer
  I want to manage my account settings and profile
  So that I can keep my information up-to-date

  Background:
    Given the storefront application is running
    And I am logged in as a customer

  @smoke @profile
  Scenario: View account dashboard
    Given I am on the homepage
    When I navigate to my account page
    Then I should see my account dashboard
    And I should see my profile information
    And I should see quick links to orders
    And I should see quick links to addresses
    And I should see quick links to settings

  @profile
  Scenario: Update profile information
    Given I am on my account page
    When I click "Edit Profile"
    And I update my first name to "Jane"
    And I update my last name to "Smith"
    And I update my phone number to "+1234567890"
    And I click "Save Changes"
    Then I should see a success message
    And my profile should display the updated information

  @validation
  Scenario: Profile form validates required fields
    Given I am on my account page
    When I click "Edit Profile"
    And I clear the first name field
    And I click "Save Changes"
    Then I should see "First name is required" error
    And the form should not be submitted

  @addresses
  Scenario: Add a new shipping address
    Given I am on my account page
    When I go to "Addresses" section
    And I click "Add New Address"
    And I fill in the address form:
      | Field          | Value              |
      | Street Address | 123 Main St        |
      | City           | New York           |
      | State          | NY                 |
      | Postal Code    | 10001              |
      | Country        | United States      |
    And I click "Save Address"
    Then I should see the new address in my addresses list

  @addresses
  Scenario: Set default shipping address
    Given I have multiple addresses saved
    And I am on my account addresses page
    When I select an address
    And I click "Set as Default"
    Then that address should be marked as default
    And I should see a "Default" badge on the address

  @addresses
  Scenario: Edit existing address
    Given I have a saved address
    And I am on my account addresses page
    When I click "Edit" on an address
    And I update the street address to "456 Oak Avenue"
    And I click "Save Changes"
    Then the address should be updated
    And I should see "456 Oak Avenue" in my addresses

  @addresses
  Scenario: Delete an address
    Given I have multiple addresses saved
    And I am on my account addresses page
    When I click "Delete" on an address
    Then I should see a confirmation dialog
    When I confirm the deletion
    Then the address should be removed from my list

  @settings
  Scenario: Change email preferences
    Given I am on my account page
    When I go to "Settings" section
    And I toggle "Receive promotional emails" to off
    And I toggle "Order updates" to on
    And I click "Save Preferences"
    Then my email preferences should be updated
    And I should see a confirmation message

  @settings
  Scenario: Enable two-factor authentication
    Given I am on my account settings page
    When I click "Enable Two-Factor Authentication"
    Then I should see setup instructions
    And I should see a QR code
    When I enter the verification code
    And I click "Verify"
    Then 2FA should be enabled for my account

  @settings
  Scenario: Change password redirect
    Given I am on my account settings page
    When I click "Change Password"
    Then I should be redirected to Keycloak
    And I should see the password change form

  @privacy
  Scenario: Download my data
    Given I am on my account settings page
    When I click "Download My Data"
    Then I should see a data export request confirmation
    And I should receive an email when data is ready
    And I can download a ZIP file with my data

  @privacy
  Scenario: Delete my account
    Given I am on my account settings page
    When I click "Delete Account"
    Then I should see a warning about account deletion
    When I confirm account deletion
    And I enter my password
    And I click "Delete My Account"
    Then my account should be marked for deletion
    And I should be logged out
    And I should receive a confirmation email

  @wishlist
  Scenario: View saved wishlist items
    Given I have items in my wishlist
    When I navigate to my account page
    And I go to "Wishlist" section
    Then I should see all my wishlist items
    And each item should show title, price, and availability

  @wishlist
  Scenario: Remove item from wishlist
    Given I am viewing my wishlist
    When I click "Remove" on a wishlist item
    Then the item should be removed from my wishlist
    And the wishlist count should decrease

  @wishlist
  Scenario: Add wishlist item to basket
    Given I am viewing my wishlist
    When I click "Add to Basket" on a wishlist item
    Then the item should be added to my basket
    And the item should remain in my wishlist

  @order-history-link
  Scenario: Navigate to order history from account
    Given I am on my account page
    When I click "View All Orders"
    Then I should be redirected to my orders page

  @mobile-account
  Scenario: Account management on mobile
    Given I am on a mobile device
    And I am on my account page
    Then the account sections should be mobile-friendly
    And I can navigate between sections easily
    And all actions should be touch-optimized

  @accessibility
  Scenario: Account page keyboard navigation
    Given I am on my account page
    When I navigate using only keyboard
    Then all sections should be accessible
    And all forms should be keyboard-navigable
    And focus indicators should be visible
