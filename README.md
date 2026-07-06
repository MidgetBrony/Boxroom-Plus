# Boxroom Plus

**Version 1.2.1**

Boxroom Plus extends **BOXROOM** with support for custom applications,
emulators, and non-Steam games while protecting custom entries from
Steam cache refreshes.

Unlike vanilla BOXROOM, Boxroom Plus allows custom AppIDs to behave like
native Steam titles inside your library.

------------------------------------------------------------------------

# Features

-   Launch custom executables using `launch.json`
-   Launch emulators with ROM arguments
-   Launch non-Steam games
-   Treat custom AppIDs as owned by Steam
-   Prevent custom games from being removed during cache refreshes
-   Prevent metadata invalidation for custom games
-   Automatic fallback to Steam launching
-   Fully compatible with existing Steam titles

------------------------------------------------------------------------

# Requirements

-   BOXROOM
-   MelonLoader
-   .NET Framework compatible with BOXROOM

------------------------------------------------------------------------

# Installing MelonLoader

1.  Download MelonLoader from https://melonwiki.xyz
2.  Run the installer.
3.  Select `BOXROOM.exe`.
4.  Install the latest stable version.
5.  Launch BOXROOM once.
6.  Verify the following folders exist:

``` text
BOXROOM/
├── Mods/
├── UserData/
└── MelonLoader/
```

7.  Close BOXROOM.

------------------------------------------------------------------------

# Installing Boxroom Plus

Copy:

``` text
BoxroomPlus.dll
```

into:

``` text
BOXROOM/Mods/
```

Launch BOXROOM.

If installed correctly you should see:

``` text
Boxroom Plus Loaded!
```

inside the MelonLoader console.

------------------------------------------------------------------------

# How It Works

## Steam Games

``` text
BOXROOM
    │
    ▼
Steam Launch
```

## Custom Games

``` text
BOXROOM
    │
    ▼
launch.json Found?
    │
 ┌──┴─────┐
 │ Yes    │ No
 ▼        ▼
Launch    Steam
Executable Launch
```

------------------------------------------------------------------------

# Custom AppIDs

Boxroom Plus patches BOXROOM's ownership validation so custom AppIDs are
treated as owned.

This allows custom games to appear and launch normally without requiring
a Steam license for the custom AppID.

Steam games continue using Steam's normal ownership validation.

------------------------------------------------------------------------

# Cache Protection

During Steam cache refreshes BOXROOM may invalidate cached metadata.

Boxroom Plus skips invalidation for custom AppIDs, protecting:

-   Custom metadata
-   Custom launch configurations
-   Imported custom games

This prevents custom entries from being removed during refresh
operations.

------------------------------------------------------------------------

# launch.json

Place a `launch.json` file inside:

``` text
AppData\LocalLow\NestedLoop\BOXROOM\steam_cache_v2\<AppId>\
```

Example:

``` text
steam_cache_v2/
└── 900000001/
    ├── game.json
    ├── screenshots/
    └── launch.json
```

------------------------------------------------------------------------

# Example

``` json
{
  "Executable": "C:\\Games\\Minecraft\\MinecraftLauncher.exe",
  "Arguments": "",
  "WorkingDirectory": "C:\\Games\\Minecraft",
  "UseShellExecute": true
}
```

------------------------------------------------------------------------

# launch.json Fields

## Executable

Required path to the executable.

## Arguments

Optional command-line arguments.

## WorkingDirectory

Optional working directory.

If omitted, Boxroom Plus automatically uses the executable's directory.

## UseShellExecute

Optional.

Defaults to `true`.

------------------------------------------------------------------------

# Linux / Steam Deck

Boxroom Plus works under Proton.

Steam launch requests function normally, however launching native Linux
executables directly from Proton may not always work.

## Recommended Workaround

1.  Add the Linux application as a **Non-Steam Game**.
2.  Create a desktop shortcut from Steam.
3.  Open the generated `.desktop` file.
4.  Copy the generated launch command into `launch.json`.

Steam will then handle launching the application correctly.

------------------------------------------------------------------------

# Companion Application

## Boxroom Studio

Boxroom Studio provides a graphical interface for managing custom games.

Features include:

-   Create custom games
-   Import metadata
-   Download SteamGridDB artwork
-   Download screenshots
-   Generate and edit `launch.json`
-   Manage custom metadata

GitHub:

https://github.com/MidgetBrony/Boxroom-Studio

------------------------------------------------------------------------

# Planned

-   Improved Boxroom Studio integration
-	Additional Linux Patches

------------------------------------------------------------------------

# License

MIT License
