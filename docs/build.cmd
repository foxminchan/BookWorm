@echo off
REM Build the documentation using DocFX

echo Building BookWorm Architecture Documentation...

REM Ensure we're in the docs directory
cd /d "%~dp0"

REM Clean previous build
if exist "_site" (
    echo Cleaning previous build...
    rmdir /s /q "_site"
)

REM Build documentation
echo Building documentation...
docfx build docfx.json --maxParallelism 1

if %ERRORLEVEL% EQU 0 (
    echo ✅ Documentation built successfully!
    echo 📁 Output directory: _site
    echo 🌐 Open _site/index.html in your browser to view the documentation
) else (
    echo ❌ Documentation build failed!
    exit /b 1
)