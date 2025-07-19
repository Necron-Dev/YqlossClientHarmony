![YqlossClientHarmony](https://socialify.git.ci/Necron-Dev/YqlossClientHarmony/image?custom_description=An+ADOFAI+Mod+created+by+Yqloss+%E2%99%A5&custom_language=C%23&description=1&font=JetBrains+Mono&forks=1&issues=1&language=1&name=1&pulls=1&stargazers=1&theme=Auto)

# Yqloss Client Harmony

> A Mod for A Dance of Fire and Ice.
>
> *Named after Yqloss Client Mixin.*

## ✨ Features

* Fix Killer Decorations Failing The Game In No Fail Mode

* Fix Set Input Event Crashing Levels (Making Them Unplayable)

* Revert Changes To Pause Events On Counterclockwise U-Turns In 2.9.4

* Modify Loading Level (basically an effect remover)

  > The level is modified when it's read from file, so the game doesn't even
  > know what it looks like before modifications. Thus, you cannot save a
  > modified level, or the file would be overwritten by the version after
  > modifications (unless you intend to do that).
  >
  > This also makes loading full-effect maps as if you are loading no-effect.
  > As fast and as low memory consumption.

* Replay

  > Things that are not supported: any input devices other than keyboards,
  > DLC contents (partially supported), official levels
  >
  > Please use "Normal" hold tile behavior when recording and playing replays.
  >
  > I've tried my best to keep the judgements and accuracy consistent
  > with the original play.
  >
  > (7BG what the f are you doing in your code??????)

## ⚙️ How To Build

Install Unity Mod Manager in your ADOFAI.

Create `GameFolder.txt` in the project folder and write the path to your game folder (used by the installation scripts).

The content would be like:

```text
D:\Program Files (x86)\Steam\steamapps\common\A Dance of Fire and Ice
```

Copy your ADOFAI game folder (generally located in `Steam/steamapps/common`) to `ADOFAIGame`.

The project folder structure would be like:

```text
YqlossClientHarmony
|-- GameFolder.txt
|-- ADOFAIGame
|   |-- A Dance of Fire and Ice
|       |-- A Dance of Fire and Ice.exe
|       |-- A Dance of Fire and Ice_Data
|       |   |-- Managed
|       |   |   |-- UnityModManager
|       |   |   |   |-- ...
|       |   |   |-- ...
|       |   |-- ...
|       |-- ...
|-- ...
```

Build the project with **Release** build profile.

Run Scripts/pack.ps1 with PowerShell if you want to make a Mod zip file. The zip file would be generated at
`build/YCH.zip` in the project folder.

Run Scripts/install.ps1 with PowerShell if you want to install the Mod directly into your game.

Don't worry if errors are printed during script execution.

## 📄 License

This project is licensed under the **GNU General Public License v2.0 (GPLv2)**.

See the [LICENSE](LICENSE) file for details.
