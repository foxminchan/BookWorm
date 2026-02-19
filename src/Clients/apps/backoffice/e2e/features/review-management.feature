@rating
@reviews
@p2
Feature: Review Management
  As an admin
  I want to manage customer reviews
  So that I can maintain quality and appropriate content

  Background:
    Given the backoffice application is running
    And I am logged in as an admin
    And I am on the reviews page

  @smoke
  @view-reviews
  Scenario: View list of reviews
    When the reviews page loads
    Then I should see the reviews table
    And I should see review information including rating and status

  @search
  @reviews
  Scenario: Search for reviews by book title
    Given there are multiple reviews
    When I search for reviews by book title
    Then I should see only matching reviews

  @approve-review
  Scenario: Approve a pending review
    Given there is at least one pending review
    When I click approve on the first review
    Then the review should be approved
    And I should see a success message

  @reject-review
  Scenario: Reject an inappropriate review
    Given there is at least one pending review
    When I click reject on the first review
    Then the review should be rejected
    And I should see a success message

  @filter
  @reviews
  Scenario: Filter reviews by status
    Given there are reviews with different statuses
    When I filter reviews by "Pending" status
    Then I should see only pending reviews

  @filter
  @reviews
  Scenario: Filter reviews by rating
    Given there are reviews with different ratings
    When I filter reviews by 5-star rating
    Then I should see only 5-star reviews

  @pagination
  @reviews
  Scenario: Navigate through review pages
    Given there are more than 10 reviews
    When I navigate to page 2
    Then I should see different reviews
    And the pagination should update
