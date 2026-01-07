@high-priority @ai-features @chatbot
Feature: AI-Powered Book Recommendations
  As a customer
  I want to interact with an AI chatbot
  So that I can get personalized book recommendations and assistance

  Background:
    Given the storefront application is running
    And AI features are enabled

  @smoke @chat
  Scenario: Open AI chatbot from homepage
    Given I am on the homepage
    When I click "Try AI Recommendations" button
    Then the AI chatbot dialog should open
    And the chatbot should be focused
    And I should see the welcome message "Hi! How can I help you find a book today?"

  @smoke @chat
  Scenario: Open AI chatbot from floating button
    Given I am on any page
    When I click the floating chat button with label "Open chat"
    Then the AI chatbot dialog should open with title "BookWorm Literary Guide"
    And the chatbot should be ready to receive messages
    And I should see the placeholder text "Ask about a book..."

  @chat-interaction
  Scenario: Get book recommendation based on mood
    Given the AI chatbot is open
    When I type "I want something uplifting and inspiring"
    And I send the message
    Then I should receive AI-generated book recommendations
    And the recommendations should include book titles
    And each recommendation should have a brief description

  @chat-interaction
  Scenario: Ask for books similar to a specific title
    Given the AI chatbot is open
    When I type "I loved 'The Design of Everyday Things', what else would I enjoy?"
    And I send the message
    Then I should receive relevant book suggestions
    And the response should explain why the books are similar

  @chat-interaction
  Scenario: Search for books by topic
    Given the AI chatbot is open
    When I type "Can you recommend books about sustainable design?"
    And I send the message
    Then I should receive topic-specific recommendations
    And I should be able to click on book titles to view details

  @chat-navigation
  Scenario: Navigate to book from chatbot recommendation
    Given the AI chatbot is open
    And I received book recommendations
    When I click on a recommended book title
    Then I should be redirected to that book's detail page
    And the chatbot should remain available

  @chat-history
  Scenario: View conversation history
    Given the AI chatbot is open
    And I have sent multiple messages
    When I scroll up in the chat
    Then I should see my previous messages
    And I should see all AI responses
    And the conversation should be in chronological order

  @chat-clear
  Scenario: Clear chat history
    Given the AI chatbot is open
    And I have an existing conversation
    When I click "Clear Chat"
    Then the chat history should be cleared
    And I should see a fresh welcome message

  @accessibility @keyboard-nav
  Scenario: Navigate chatbot with keyboard
    Given I am on the homepage
    When I press Tab until the chat button is focused
    And I press Enter
    Then the chatbot dialog should open
    When I type a message
    And I press Tab to the send button
    And I press Enter
    Then the message should be sent

  @accessibility @focus-trap
  Scenario: Chatbot dialog traps focus
    Given the AI chatbot is open
    When I press Tab repeatedly
    Then focus should cycle within the dialog
    And focus should not escape to the page behind

  @chat-close
  Scenario: Close chatbot with escape key or close button
    Given the AI chatbot is open
    When I press Escape or click the close button
    Then the chatbot should close
    And focus should return to the trigger button

  @error-handling @feature-disabled
  Scenario: Show unavailable message when feature is disabled
    Given AI features are disabled
    When I click the floating chat button
    Then I should see an "unavailable" dialog
    And the dialog should show "Feature in Development" title
    And I should see message "Our AI chat assistant is currently under development and will be available soon. Stay tuned!"
    And I can close the dialog by clicking outside or the close button

  @error-handling @gateway-unavailable
  Scenario: Show unavailable message when gateway is not configured
    Given the API gateway is not configured
    When I click the floating chat button
    Then I should see an "unavailable" dialog with title "Chat Unavailable"
    And I should see message "The chat feature is currently unavailable. Please try again later."

  @error-handling
  Scenario: Handle AI service unavailability
    Given the AI chatbot is open
    And the AI service is unavailable
    When I send a message
    Then I should see an error notification
    And I should be prompted to try again later

  @rate-limiting
  Scenario: Handle rate limiting gracefully
    Given the AI chatbot is open
    And I have reached the rate limit
    When I send another message
    Then I should see a rate limit message
    And I should be informed when I can send again

  @mobile-experience
  Scenario: Chatbot is responsive on mobile
    Given I am on a mobile device
    And I am on the homepage
    Then the floating chat button should be hidden on mobile
    And the chatbot is only available on desktop screens

  @chat-ui
  Scenario: Chatbot uses CopilotSidebar component
    Given I am on a desktop device
    When I open the AI chatbot
    Then the chatbot should use the CopilotSidebar component
    And it should support click outside to close
    And the sidebar should be positioned fixed with full-screen overlay
