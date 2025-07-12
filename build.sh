#!/bin/bash

# PEAK Unlimited Build Script for Unix Systems
# This script builds the mod and optionally copies it to the game directory

PEAK_PATH="${PEAK_PATH:-/path/to/your/PEAK/game}"
DEPLOY=false
CLEAN=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --deploy)
            DEPLOY=true
            shift
            ;;
        --clean)
            CLEAN=true
            shift
            ;;
        --path)
            PEAK_PATH="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--deploy] [--clean] [--path /path/to/PEAK]"
            exit 1
            ;;
    esac
done

echo "PEAK Unlimited Build Script"
echo "=========================="

# Set environment variable for the build
export PEAK_PATH="$PEAK_PATH"
echo "Using PEAK_PATH: $PEAK_PATH"

# Clean if requested
if [ "$CLEAN" = true ]; then
    echo "Cleaning build artifacts..."
    dotnet clean
    rm -rf src/PEAKUnlimited/bin
    rm -rf src/PEAKUnlimited/obj
fi

# Restore packages
echo "Restoring packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "Package restore failed!"
    exit 1
fi

# Build the project
echo "Building project..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

echo "Build completed successfully!"

# Deploy if requested
if [ "$DEPLOY" = true ]; then
    PLUGIN_PATH="src/PEAKUnlimited/bin/Release/netstandard2.1/PEAKUnlimited.dll"
    TARGET_PATH="$PEAK_PATH/BepInEx/plugins/PEAKUnlimited.dll"
    
    if [ -f "$PLUGIN_PATH" ]; then
        if [ -d "$PEAK_PATH/BepInEx/plugins" ]; then
            echo "Deploying to game directory..."
            cp "$PLUGIN_PATH" "$TARGET_PATH"
            echo "Deployed to: $TARGET_PATH"
        else
            echo "BepInEx plugins directory not found. Please install BepInEx first."
        fi
    else
        echo "Plugin DLL not found at: $PLUGIN_PATH"
    fi
fi

echo "Build script completed!" 