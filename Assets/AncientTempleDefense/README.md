# Ancient Temple Defense - Environment Art

This folder contains the first production environment assets for the dark-fantasy 2D defense game.

## Assets

- `Art/Environment/dark_temple_valley_3840x1080.png`
  - Exact size: 3840 x 1080 RGB.
  - Long 32:9 combat map with enemy approach routes on both sides.
  - The central area is deliberately quieter for the temple base.
- `Art/Environment/ancient_temple_base.png`
  - Exact size: 1536 x 1024 RGBA.
  - Transparent standalone temple/base sprite.
  - Bottom-center pivot for simple placement on the combat ground.

Both textures use Point filtering, no mipmaps, and uncompressed Standalone import settings. The background permits a 4096-pixel maximum size so Unity does not shrink the 3840-pixel source.

## Suggested Unity placement

- Keep both sprites at 100 Pixels Per Unit.
- Center the background at world `(0, 0)`; its world size is `38.4 x 10.8` units.
- Put the temple at world `x = 0`, align its bottom pivot to the combat floor, and begin with local scale `(0.65, 0.65, 1)`.
- Suggested sorting: background `-20`, temple `0`, characters/enemies `10`, foreground effects `20`.
- For a 1920 x 1080 gameplay camera, use orthographic size `5.4` and allow horizontal camera travel across the 3840-pixel map.

## Visual direction derived from the imported packs

- Chunky hard-edged pixel clusters and Point filtering.
- Near-black and navy shadows with muted violet midtones.
- Restrained rust-orange highlights that echo the Black Knight weapon effects.
- A readable, low-contrast combat band for the Martial Hero and 150 x 150 monster frames.
- No characters from the imported packs were copied into the generated environment art.

## Final generation prompt set

Built-in ImageGen was used. Imported pack images were inspected locally but were not sent to the generation service.

### Background

Create a continuous 32:9 dark-fantasy pixel-art ruined sacred valley for a 2D side-scrolling fighting and defense game. Use a true side view, layered mountains and ruined monoliths, enemy approach routes from both sides, a quiet central placement zone, and a mostly level bottom combat strip. Render deliberate chunky pixels, crisp silhouettes, near-black navy and muted violet stone with restrained rust-orange runes. No characters, temple, UI, text, smooth gradients, photorealism, or isometric perspective.

### Temple base

Create an isolated side-view ancient sanctuary made from cracked black basalt and eroded sandstone, with broken guardian pillars, broad stepped foundation, central sealed altar, restrained runes, chains, roots, and a small orange-red sacred core. Use handcrafted hard-edged pixel art and a strong readable silhouette. No terrain scene, characters, UI, text, modern elements, photorealism, or isometric perspective.

### Transparency correction

Preserve the generated temple exactly and replace only the exterior background with uniform `#ff00ff`, with no gradient, glow, shadow, fog, texture, floor, reflection, or border. The chroma source was converted locally to RGBA with the installed ImageGen background-removal helper.
