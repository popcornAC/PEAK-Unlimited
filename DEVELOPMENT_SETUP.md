# PEAK Unlimited - Development Setup Guide

> **IMPORTANT:**
>
> - **Do NOT commit any DLLs from the `libs/` directory to the repository.**
> - These files are proprietary (Unity, game, BepInEx) and must be obtained from your own installation.
> - The `.gitignore` is configured to prevent this, but double-check before pushing.

## Prerequisites

1. **Visual Studio 2022** or **Visual Studio Code** with C# extensions
2. **PEAK Game** installed via Steam
3. **BepInEx** installed in your PEAK game directory
4. **.NET 6.0 SDK** or later

## Initial Setup

### 1. Install BepInEx

1. Download the latest BepInEx from: https://github.com/BepInEx/BepInEx/releases
2. Extract to your PEAK game directory: `C:\Program Files (x86)\Steam\steamapps\common\PEAK`
3. Run the game once to initialize BepInEx

### 2. Configure Game Path

The project uses a `PEAK_PATH` environment variable to locate game files. You can set this in several ways:

**Option A: Environment Variable**

```bash
# Windows
set PEAK_PATH=C:\Program Files (x86)\Steam\steamapps\common\PEAK

# macOS/Linux
export PEAK_PATH="/path/to/your/PEAK/game"
```

**Option B: Modify Project File**
Edit `src/PEAKUnlimited/PEAKUnlimited.csproj` and change the default path:

```xml
<PEAK_PATH Condition="'$(PEAK_PATH)' == ''">YOUR_ACTUAL_PEAK_PATH</PEAK_PATH>
```

### 3. Build the Project

```bash
dotnet restore
dotnet build
```

## Development Workflow

### Building

- Use `dotnet build` to compile the mod
- The mod will automatically copy to `$(PEAK_PATH)\BepInEx\plugins` if the directory exists

### Testing

1. Build the project
2. Start PEAK
3. Check the BepInEx console for mod loading messages
4. Test multiplayer functionality

### Debugging

1. Attach your debugger to the PEAK process
2. Set breakpoints in your code
3. Use `Logger.LogInfo()` for console output

## Project Structure

```
PEAK-Unlimited/
├── src/PEAKUnlimited/
│   ├── Plugin.cs              # Main plugin logic
│   ├── PEAKUnlimited.csproj   # Project configuration
│   └── thunderstore.toml      # Thunderstore build config
├── NuGet.config               # Package sources
├── manifest.json              # Mod metadata
└── README.md                  # User documentation
```

## Key Features to Understand

### Configuration System

The plugin uses BepInEx's configuration system. Settings are defined in `Plugin.cs` and saved to:
`$(PEAK_PATH)\BepInEx\config\PeakUnlimited.cfg`

### Harmony Patching

The mod uses Harmony to patch game methods:

- `AwakePatch`: Handles campfire initialization
- `OnPlayerEnteredRoomPatch`: Manages player joins
- `OnPlayerLeftRoomPatch`: Manages player leaves
- `EndScreen*Patch`: Handles end screen UI

### Multiplayer Considerations

- Only the host needs the mod installed
- Most logic runs on the master client (host)
- Uses Photon networking for multiplayer

## Common Issues

### Build Errors

- **Missing references**: Ensure PEAK_PATH is set correctly
- **C# version**: Project uses C# 10.0 features
- **Dependencies**: Run `dotnet restore` to download packages

### Runtime Issues

- **Mod not loading**: Check BepInEx console for errors
- **Missing marshmallows**: Verify configuration settings
- **UI issues**: Check if end screen patches are working
- **End screen crashes**: May indicate game version incompatibility

### Troubleshooting Steps

1. **Clean build**: Run `dotnet clean` then `dotnet build`
2. **Update DLLs**: Run `.\extract_dlls.ps1` to get latest game DLLs
3. **Check BepInEx version**: Ensure you're using a compatible BepInEx version
4. **Verify game path**: Make sure PEAK_PATH points to your actual game installation

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## Resources

- [BepInEx Documentation](https://docs.bepinex.dev/)
- [Harmony Documentation](https://harmony.pardeike.net/)
- [PEAK Game](https://store.steampowered.com/app/1628970/PEAK/)
- [Thunderstore](https://thunderstore.io/c/peak/)
