@medium-priority @content-pages @informational
Feature: Content and Information Pages
  As a customer
  I want to access company information and policies
  So that I can learn about BookWorm and understand shipping and return policies

  Background:
    Given the storefront application is running

  @smoke @about
  Scenario: View About Us page
    Given I am on the homepage
    When I click "About" in the footer
    Then I should be on the about page
    And I should see the company mission statement
    And I should see the values section
    And I should see the company timeline
    And I should see contact information

  @about @sections
  Scenario: Navigate through About page sections
    Given I am on the about page
    When I scroll through the page
    Then I should see the hero section
    And I should see the mission section
    And I should see the values section
    And I should see the timeline section
    And I should see the contact section

  @about @accessibility
  Scenario: About page is keyboard navigable
    Given I am on the about page
    When I navigate using Tab key
    Then all interactive elements should be focusable
    And section headings should be properly structured

  @shipping @smoke
  Scenario: View Shipping Information page
    Given I am on the homepage
    When I click "Shipping" in the footer
    Then I should be on the shipping page
    And I should see shipping methods
    And I should see delivery timeframes
    And I should see shipping costs information
    And I should see international shipping details

  @shipping
  Scenario: Access shipping info from basket
    Given I have items in my basket
    And I am on the basket page
    When I click "Shipping Information" link
    Then I should be redirected to the shipping page
    And I should be able to return to basket

  @returns @smoke
  Scenario: View Returns & Refunds policy
    Given I am on the homepage
    When I click "Returns" in the footer
    Then I should be on the returns page
    And I should see the return policy
    And I should see the refund process
    And I should see the time window for returns
    And I should see contact information for returns

  @returns
  Scenario: Access returns policy from order details
    Given I am logged in
    And I have a completed order
    When I view the order details
    And I click "Return Policy" link
    Then I should be redirected to the returns page

  @footer-navigation
  Scenario: All footer links are functional
    Given I am on any page
    When I scroll to the footer
    Then I should see links to "About"
    And I should see links to "Shipping"
    And I should see links to "Returns"
    And I should see links to "Privacy Policy"
    And I should see links to "Terms of Service"
    And all links should be clickable

  @privacy-policy
  Scenario: View Privacy Policy dialog
    Given I am on the homepage
    When I click "Privacy Policy" in the footer
    Then a privacy policy dialog should open
    And I should see GDPR information
    And I should see data collection details
    And I should be able to close the dialog

  @terms-of-service
  Scenario: View Terms of Service dialog
    Given I am on the homepage
    When I click "Terms of Service" in the footer
    Then a terms of service dialog should open
    And I should see usage terms
    And I should see limitation of liability
    And I should be able to close the dialog

  @contact
  Scenario: Contact form submission from About page
    Given I am on the about page
    When I scroll to the contact section
    And I fill in the contact form with valid data
    And I submit the form
    Then I should see a success message
    And I should receive a confirmation email

  @contact @validation
  Scenario: Contact form validates required fields
    Given I am on the about page
    When I scroll to the contact section
    And I submit the form without filling fields
    Then I should see validation errors
    And the form should not be submitted

  @breadcrumbs
  Scenario: Breadcrumb navigation on content pages
    Given I am on the shipping page
    Then I should see breadcrumbs
    And breadcrumbs should show "Home > Shipping"
    When I click "Home" in breadcrumbs
    Then I should return to the homepage

  @seo @metadata
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
    And images should be properly sized
    And navigation should be accessible

  @print-friendly
  Scenario: Shipping and returns pages are print-friendly
    Given I am on the shipping page
    When I open the print dialog
    Then the page should have a print-optimized layout
    And unnecessary elements should be hidden

  @newsletter-signup
  Scenario: Newsletter signup from footer
    Given I am on any page
    When I scroll to the footer
    And I enter my email in the newsletter field
    And I click "Subscribe"
    Then I should see a subscription confirmation
    And I should receive a welcome email

  @social-media-links
  Scenario: Social media links in footer work
    Given I am on the homepage
    When I scroll to the footer
    Then I should see social media icons
    And clicking Twitter should open BookWorm's Twitter
    And clicking Instagram should open BookWorm's Instagram
    And links should open in new tabs
