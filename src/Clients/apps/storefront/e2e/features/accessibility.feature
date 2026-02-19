@accessibility
@a11y
Feature: Accessibility and Keyboard Navigation
  As a user who relies on assistive technology
  I want the site to be fully accessible
  So that I can independently browse and purchase books

  Background:
    Given the storefront application is running
    And the catalog has available books

  @critical
  @keyboard-nav
  Scenario: Skip to main content link is accessible
    Given I am on the homepage
    When I press Tab key once
    Then the "Skip to main content" link should be focused
    And it should be visible when focused
    When I press Enter
    Then focus should move to main content area

  @critical
  @keyboard-nav
  Scenario: Complete checkout using keyboard only
    Given I am on the homepage
    When I navigate to shop using keyboard
    And I select first book using keyboard
    And I add to basket using keyboard
    And I navigate to basket using keyboard
    And I proceed to checkout using keyboard
    Then I should complete the checkout successfully
    And all interactive elements should have been keyboard accessible

  @focus-management
  Scenario: Focus management in remove item dialog
    Given I have 1 book in my basket
    And I am on the basket page
    When I press Tab until remove button is focused
    And I press Enter to open remove dialog
    Then focus should move to the dialog
    And the dialog should have role="dialog" or role="alertdialog"
    When I press Tab
    Then focus should cycle within the dialog only
    When I press Escape
    Then the dialog should close
    And focus should return to the remove button

  @focus-management
  Scenario: Focus management in search functionality
    Given I am on the shop page
    When I click the search button
    Then the search input should be automatically focused
    When I press Escape
    Then the search input should lose focus

  @aria-labels
  Scenario: Interactive elements have proper accessible names
    Given I am on the shop page
    When I inspect the page elements
    Then all buttons should have accessible names
    And all form inputs should have associated labels
    And all images should have descriptive alt text
    And the basket icon should announce "Shopping basket, X items"

  @aria-live
  Scenario: Dynamic updates are announced to screen readers
    Given I am on a product detail page
    When I add item to basket
    Then an aria-live region should announce the update
    And the basket count should be updated

  Scenario: Filter changes are announced
    Given I am on the shop page
    When I select a category filter
    Then the results count should be announced
    And the aria-live region should update

  @form-validation
  Scenario: Form errors are accessible
    Given I am on a product detail page
    When I scroll to the reviews section
    And I click "Write a Review"
    And I submit the form without filling required fields
    Then each error should have aria-describedby linking to error message
    And the first error should be announced
    And focus should move to first invalid field

  @loading-states
  Scenario: Loading states are announced
    Given I am on the shop page
    When the books are loading
    Then loading skeletons should have role="status"
    And aria-label="Loading books" or similar
    When books finish loading
    Then the content should have proper semantic structure

  @landmarks
  Scenario: Page has proper landmark regions
    Given I am on any page
    Then the page should have:
      | landmark    | count |
      | main        | 1     |
      | navigation  | 1-2   |
      | contentinfo | 1     |
    And each landmark should have proper aria-label if multiple of same type

  @color-contrast
  Scenario: Interactive elements have sufficient color contrast
    # This would typically be done with axe-core
    Given I am on the homepage
    When I run automated accessibility scan
    Then there should be no color contrast violations
    And WCAG AA standards should be met

  @mobile-a11y
  Scenario: Mobile navigation is accessible with touch and screen readers
    Given I am on the homepage on mobile viewport
    When I navigate using touch
    Then the mobile bottom nav should have proper labels
    And touch targets should be at least 44x44 pixels
    And all interactive elements should be reachable

  @heading-structure
  Scenario: Pages have proper heading hierarchy
    Given I am on the shop page
    Then headings should follow logical order (h1, h2, h3)
    And there should be only one h1 per page
    And no heading levels should be skipped
