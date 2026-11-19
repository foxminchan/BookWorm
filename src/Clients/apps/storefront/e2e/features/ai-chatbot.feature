@high-priority
@ai-features
@chatbot
Feature: AI-Powered Book Recommendations
  As a customer
  I want to interact with an AI chatbot
  So that I can get personalized book recommendations and assistance

  Background:
    Given the storefront application is running
    And I am logged in as a customer
    And AI features are enabled

  @smoke
  @chat
  Scenario: Open AI chatbot from homepage
    Given I am on the homepage
    When I click "Try AI Recommendations" button
    Then the AI chatbot dialog should open
    And I should see the welcome message "Hi! I'm your literary assistant. I can help you find books, manage your basket, and answer questions about our collection. What would you like to explore today?"

  @smoke
  @chat
  Scenario: Open AI chatbot from floating button
    Given I am on any page
    When I click the floating chat button with label "Open BookWorm Literary Guide chat"
    Then the AI chatbot dialog should open with title "BookWorm Literary Guide"
    And the chatbot should be ready to receive messages
    And I should see the placeholder text "Ask about books, search for titles, or manage your basket..."

  @chat-interaction
  Scenario: Get book recommendation based on mood
    Given the AI chatbot is open
    When I type "I want something uplifting and inspiring"
    And I send the message
    Then I should receive AI-generated book recommendations

  @chat-interaction
  Scenario: Ask for books similar to a specific title
    Given the AI chatbot is open
    When I type "I loved 'The Design of Everyday Things', what else would I enjoy?"
    And I send the message
    Then I should receive relevant book suggestions

  @chat-history
  Scenario: View conversation history
    Given the AI chatbot is open
    And I have sent multiple messages
    When I scroll up in the chat
    Then I should see my previous messages
    And I should see all AI responses

  @accessibility
  @keyboard-nav
  Scenario: Navigate chatbot with keyboard
    Given I am on the homepage
    When I press Tab until the chat button is focused
    And I press Enter
    Then the chatbot dialog should open

  @accessibility
  @focus-management
  Scenario: Focus returns to trigger button after closing dialog
    Given I am on the homepage
    When I click the floating chat button
    Then the AI chatbot dialog should open
    When I press Escape
    Then the chatbot should close
    And focus should return to the trigger button

  @error-handling
  @unauthenticated
  Scenario: Show sign in prompt when unauthenticated
    Given I am not logged in
    When I click the unauthenticated chat button
    Then I should see an "unavailable" dialog with title "Sign in to Chat"
    And I should see "Please sign in to your account to chat with our AI literary assistant." message
    And I should see "Sign In" link
    And I can close the unavailable dialog

  @error-handling
  @feature-disabled
  Scenario: Show unavailable message when feature is disabled
    Given AI features are disabled
    When I click the disabled chat button
    Then I should see an "unavailable" dialog with title "Feature in Development"
    And I should see "Our AI chat assistant is currently under development and will be available soon. Stay tuned!" message
    And I can close the unavailable dialog

  @error-handling
  @gateway-unavailable
  Scenario: Show unavailable message when gateway is not configured
    Given the API gateway is not configured
    When I click the gateway-unavailable chat button
    Then I should see an "unavailable" dialog with title "Chat Unavailable"
    And I should see "The chat feature is currently unavailable. Please try again later." message

  @error-handling
  Scenario: Handle AI service unavailability
    Given the AI chatbot is open
    And the AI service is unavailable
    When I send a message
    Then I should see an error notification

  @rate-limiting
  Scenario: Handle rate limiting gracefully
    Given the AI chatbot is open
    And I have reached the rate limit
    When I send another message
    Then I should see a rate limit message
