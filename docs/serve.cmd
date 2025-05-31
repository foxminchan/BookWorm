@echo off
REM Serve the documentation locally using DocFX

echo Starting DocFX development server...

REM Ensure we're in the docs directory
cd /d "%~dp0"

REM Build and serve
echo Building and serving documentation...
echo 📖 Documentation will be available at http://localhost:8080
echo 🔄 Auto-rebuild enabled - changes will be reflected automatically
echo ⏹️ Press Ctrl+C to stop the server
echo.

docfx serve _site --port 8080