#!/usr/bin/env bash
# Check if OpenAI API Key is set in AppHost secrets
# If not set, prompt user for input and set it

set -e

PROJECT_PATH="src/Aspire/BookWorm.AppHost/BookWorm.AppHost.csproj"
SECRET_KEY="Parameters:openai-openai-apikey"

echo -e "\033[36mChecking for OpenAI API key in user secrets...\033[0m"

# Get current secrets
SECRETS=$(dotnet user-secrets list --project "$PROJECT_PATH" 2 > &1 || true)

# Check if the OpenAI API key is set
if echo "$SECRETS" | grep -q "$SECRET_KEY" ; then
    echo -e "\033[32m✓ OpenAI API key is already configured.\033[0m"
    exit 0
fi

echo -e "\033[33m⚠ OpenAI API key is not configured.\033[0m"
echo ""
echo -e "\033[36mPlease enter your OpenAI API key (or press Ctrl+C to cancel):\033[0m"
echo -e "\033[90mYou can get your API key from: https://platform.openai.com/api-keys\033[0m"
echo ""

# Read API key securely (without echoing)
read -s -p "OpenAI API Key: " API_KEY
echo ""

if [ -z "$API_KEY" ] ; then
    echo ""
    echo -e "\033[31m✗ No API key provided. Cannot proceed.\033[0m"
    echo -e "\033[33mPlease set the OpenAI API key and try again.\033[0m"
    exit 1
fi

# Set the secret
echo ""
echo -e "\033[36mSetting OpenAI API key...\033[0m"
dotnet user-secrets set "$SECRET_KEY" "$API_KEY" --project "$PROJECT_PATH"

if [ $? -eq 0 ] ; then
    echo -e "\033[32m✓ OpenAI API key has been configured successfully.\033[0m"
    exit 0
else
    echo -e "\033[31m✗ Failed to set OpenAI API key.\033[0m"
    exit 1
fi
