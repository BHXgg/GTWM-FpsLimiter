<h1 align="center">GTWM FPS Limiter</h1>

<p align="center">Frame-rate caps on hotkeys: presets 240, 60 and 45 plus three custom slots, all rebindable.</p>

<p align="center">

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE) ![Game](https://img.shields.io/badge/Get%20To%20Work-1.1.1.12-orange) ![Unity](https://img.shields.io/badge/Unity-6000.0%20Mono-black) ![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4) ![Platform](https://img.shields.io/badge/platform-Windows%20x64-lightgrey) [![Requires GTWM 1.3.1+](https://img.shields.io/badge/requires-GTWM%201.3.1%2B-2ea44f)](https://github.com/BHXgg/GTWM)

</p>

A mod for **Get To Work**, built on the [GTWM mod loader](https://github.com/BHXgg/GTWM).

## Features

- Three presets (**240 / 60 / 45 FPS**) and three **custom slots** (10–1000 FPS).
- **Every key is rebindable** in the menu, with Ctrl/Shift/Alt combinations.
- Press the active limit's key again to remove it (optional).
- VSync is turned off while a limit is on so the cap actually applies, and restored when you remove it. The game's own settings are kept in check while the limit is active.
- Remembers the last limit across restarts.
- Optional **FPS counter** and a small toast when the limit changes.

## Installing

1. Install the [GTWM loader](https://github.com/BHXgg/GTWM/releases) (1.3.1 or newer).
2. Download `GTWM-FpsLimiter-1.0.0.zip` from [Releases](https://github.com/BHXgg/GTWM-FpsLimiter/releases).
3. Extract it into the game folder (the one with `Get To Work.exe`). It lands in `GTWM\mods\FpsLimiter\`.
4. Start the game, or press **Scan for new mods** in the Mod Manager if it's already running.

## Usage

| Key | Action |
|---|---|
| `F11` | Open the menu |
| `Ctrl 1` / `Ctrl 2` / `Ctrl 3` | 240 / 60 / 45 FPS |
| `Ctrl 4` / `Ctrl 5` / `Ctrl 6` | Custom slots (default 144 / 120 / 30) |
| `Ctrl 0` | Remove the limit |

To rebind, open the menu, click a key and press the new combination. Esc cancels, Backspace clears.

## Configuration

Settings are saved to `GTWM\config\gtwm.fpslimiter.cfg` (`key = value`). Most can be changed in the menu; for the rest,
edit the file and press **Reload** on the mod in the Mod Manager.

| Key | Default | Meaning |
|---|---|---|
| `menu_key` | `F11` | Open the menu |
| `preset1_key` | `Ctrl+Alpha1` | 240 FPS |
| `custom1_fps` | `144` | Custom slot 1 value |
| `show_counter` | `false` | Show the FPS counter |
| `press_again_to_clear` | `true` | Pressing the active key removes the limit |

## Building from source

**Requirements:** Windows, the [.NET SDK](https://dotnet.microsoft.com/download) 6.0 or newer, git, and an installed
copy of the game. The loader is included as a git submodule in `external/GTWM`.

```powershell
git clone --recursive https://github.com/BHXgg/GTWM-FpsLimiter.git
cd GTWM-FpsLimiter
.\build.ps1 -GameDir "C:\path\to\Get To Work"            # build into .\dist
.\build.ps1 -GameDir "C:\path\to\Get To Work" -Install   # build and copy into the game
.\build.ps1 -Zip                                          # also produce the release zip
```

Already cloned without `--recursive`? Run `git submodule update --init` (`build.ps1` does this for you).

### Pointing the build at your game

The build compiles against the game's own assemblies (`Get To Work_Data\Managed\*.dll`), which are **not** in this
repository. It looks for the game in this order:

1. `-GameDir "X:\path\to\game"` on `build.ps1` (or `-p:GameDir=...` on `dotnet build`)
2. the `GTW_GAME_DIR` environment variable
3. the default Steam folder, `C:\Program Files (x86)\Steam\steamapps\common\Get To Work`

The game folder is the one that contains `Get To Work.exe`.

### Project layout

```
GTWM-FpsLimiter
├── src/FpsLimiter/          the mod
├── external/GTWM/       loader (submodule): API, UI kit, shared build settings
├── build.ps1            build, package, install
└── Directory.Build.props
```

Mods hot-reload: run `.\build.ps1 -Install` while the game is open and press **Reload** in the Mod Manager (or turn on
auto-reload). See the loader's [mod-writing guide](https://github.com/BHXgg/GTWM/blob/main/docs/writing-mods.md).

## AI assistance disclosure

Parts of this project were developed with the help of an AI coding assistant (Anthropic's Claude), mainly the
user-interface layout and some of the multiplayer networking code. Treat it like any other community mod: read the
code if you want to know exactly what it does, and please report anything that looks wrong.

## Disclaimer

This is an unofficial, fan-made project. It is not affiliated with, endorsed by or supported by the developers or
publishers of *Get To Work*. No game files are included in this repository or its releases; you need your own copy
of the game. Mods change how the game runs: back up your saves, and don't report bugs you hit while modded to the
game's developers.

## License

[MIT](LICENSE) © 2026 BHXgg
