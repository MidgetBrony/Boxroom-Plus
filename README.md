# Boxroom-Plus

Adds support for launching custom applications, emulators, and non-Steam games directly from BOXROOM.

Instead of always launching through Steam, Boxroom-Plus checks for a `launch.json` file inside a game's cache folder. If found, the specified executable is launched with optional arguments and working directory settings.

If no `launch.json` exists, BOXROOM behaves normally and launches through Steam.

---

## Features

- Launch custom executables
- Launch emulators with ROM arguments
- Launch non-Steam games
- Per-game launcher configuration
- Automatic fallback to normal Steam launching
- No changes required to existing Steam games

---

## Requirements

- BOXROOM
- MelonLoader
- .NET Framework compatible with BOXROOM

---

## Installing MelonLoader

1. Download MelonLoader from https://melonwiki.xyz
2. Run the MelonLoader installer.
3. Select BOXROOM.exe.
4. Install the latest stable version.
5. Launch BOXROOM once.
6. Verify the following folders were created:

```text
BOXROOM/
├── Mods/
├── UserData/
├── MelonLoader/
```

7. Close BOXROOM.

---

## Installing Boxroom-Plus

Copy:

```text
BoxroomPlus.dll
```

to:

```text
BOXROOM/Mods/
```

Launch BOXROOM.

You should see:

```text
Boxroom Plus Loaded!
```

in the MelonLoader console.

---

## launch.json

Place a `launch.json` file inside the game's cache directory:

```text
AppData\LocalLow\NestedLoop\BOXROOM\steam_cache_v2\<AppId>\
```

Example:

```text
steam_cache_v2/
└── 900000001/
    ├── game.json
    ├── screenshots
    └── launch.json
```

---

## Example: Launching Mesen

```json
{
  "Executable": "C:\\Users\\Rusty\\Downloads\\Mesen_2.2.0_Windows\\Mesen.exe",
  "Arguments": "\"C:\\Users\\Rusty\\Downloads\\Mesen_2.2.0_Windows\\Super Mario Bros. (World).zip\"",
  "WorkingDirectory": "C:\\Users\\Rusty\\Downloads\\Mesen_2.2.0_Windows",
  "UseShellExecute": true
}
```

---

## launch.json Fields

### Executable

Required path to the executable.

### Arguments

Optional command line arguments.

### WorkingDirectory

Optional working directory. If omitted, Boxroom-Plus automatically uses the executable's folder.

### UseShellExecute

Optional. Defaults to `true`.

---

## Example: RetroArch

```json
{
  "Executable": "C:\\RetroArch\\retroarch.exe",
  "Arguments": "-L \"C:\\RetroArch\\cores\\snes9x_libretro.dll\" \"D:\\ROMS\\Super Mario World.smc\""
}
```

---

## Example: Non-Steam Game

```json
{
  "Executable": "C:\\Games\\Minecraft\\MinecraftLauncher.exe"
}
```

---

## Fallback Behavior

If `launch.json` is missing:

```text
BOXROOM -> Steam Launch
```

If `launch.json` exists:

```text
BOXROOM -> Custom Executable
```

Existing Steam titles continue working exactly as before.

---

# License

MIT License