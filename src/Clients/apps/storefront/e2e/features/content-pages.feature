@medium-priority
@content-pages
@informational
Feature: Content and Information Pages
  As a customer
  I want to access company information and policies
  So that I can learn about BookWorm and understand shipping and return policies

  Background:
    Given the storefront application is running

  @smoke
  @about
  Scenario: View About Us page
    Given I am on the homepage
    When I click "Our Story" in the footer
    Then I should be on the about page
    And I should see the company mission statement
    And I should see the values section
    And I should see the company timeline
    And I should see contact information

  @about
  @sections
  Scenario: Navigate through About page sections
    Given I am on the about page
    When I scroll through the page
    Then I should see the hero section
    And I should see the mission section
    And I should see the values section
    And I should see the timeline section
    And I should see the contact section

  @about
  @accessibility
  Scenario: About page is keyboard navigable
    Given I am on the about page
    When I navigate using Tab key
    Then all interactive elements should be focusable
    And section headings should be properly structured

  @shipping
  @smoke
  Scenario: View Shipping Information page
    Given I am on the homepage
    When I click "Shipping Info" in the footer
    Then I should be on the shipping page
    And I should see shipping methods
    And I should see delivery timeframes
    And I should see shipping costs information

  @returns
  @smoke
  Scenario: View Returns & Refunds policy
    Given I am on the homepage
    When I click "Returns" in the footer
    Then I should be on the returns page
    And I should see the return policy
    And I should see the refund process
    And I should see the time window for returns

  @footer-navigation
  Scenario: All footer links are functional
    Given I am on any page
    When I scroll to the footer
    Then I should see links to "Our Story"
    And I should see links to "Shipping Info"
    And I should see links to "Returns"
    And I should see footer policy button "Privacy Policy"
    And I should see footer policy button "Terms of Service"

  @privacy-policy
  Scenario: View Privacy Policy dialog
    Given I am on the homepage
    When I click "Privacy Policy" in the footer
    Then a privacy policy dialog should open
    And I should be able to close the policy dialog

  @terms-of-service
  Scenario: View Terms of Service dialog
    Given I am on the homepage
    When I click "Terms of Service" in the footer
    Then a terms of service dialog should open
    And I should be able to close the policy dialog

  @contact
  Scenario: Contact link on About page uses mailto
    Given I am on the about page
    When I scroll to the contact section
    Then I should see the "Get In Touch" mailto link

  @seo
  @metadata
  Scenario: Content pages have proper SEO metadata
    Given I am on the about page
    Then the page should have a descriptive title
    And the page should have meta description
    And the page should have proper heading hierarchy

  @mobile-responsive
  Scenario: Content pages are mobile responsive
    Given I am on a mobile device
    When I visit the about page
    Then the content should be readable
    And navigation should be accessible
