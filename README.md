# Custom Sky

A mod for Gorilla Tag that allows you to customize and replace the sky textures.

---

<img src=".assets/preview.jpg" width="50%" alt="Custom Sky Preview" style="border-radius: 8px;" />

---

## Installation
1. Ensure you have BepInEx 5 installed.
2. Download the latest version from the [Releases](https://github.com/pl2w/CustomSky/releases/latest) page
3. Move the mod into your plugins folder.

---
   
## Usage
1. **Launch the game once** to generate the necessary folders. <br>This will export the default sky textures into `./SkyboxExport` and create a `./CustomSkies` folder.
2. Place your textures into the `./CustomSkies` folder.
   > **Important:** The file names in `./CustomSkies` must match the names of the original textures in `./SkyboxExport` exactly, or the mod will not load them.

---
      
## Configuration
After the first launch, a config file is generated in `BepInEx/config/xyz.pl2w.customsky.cfg`

| Section | Setting | Default | Description |
|---|---|---|---|
| Sky | `UseSingleTextureForAllSky` | `false` | If true, use one texture for all sky time-of-day slots instead of per-slot textures. |
| Sky | `SingleSkyTextureFileName` | `Sky_All.png` | Filename inside `./CustomSkies` used for every sky slot when `UseSingleTextureForAllSky` is true. |

---

## Disclaimer

This product is not affiliated with Another Axiom Inc. or its videogames Gorilla Tag and Orion Drift and is not endorsed or otherwise sponsored by Another Axiom.  
Portions of the materials contained herein are property of Another Axiom. ©2021 Another Axiom Inc.
