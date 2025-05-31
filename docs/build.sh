#!/bin/bash
# Build the documentation using DocFX

echo "Building BookWorm Architecture Documentation..."

# Ensure we're in the docs directory
cd "$(dirname "$0")"

# Clean previous build
if [ -d "_site" ]; then
    echo "Cleaning previous build..."
    rm -rf _site
fi

# Build documentation
echo "Building documentation..."
docfx build docfx.json --maxParallelism 1

if [ $? -eq 0 ]; then
    echo "✅ Documentation built successfully!"
    echo "📁 Output directory: _site"
    echo "🌐 Open _site/index.html in your browser to view the documentation"
else
    echo "❌ Documentation build failed!"
    exit 1
fi