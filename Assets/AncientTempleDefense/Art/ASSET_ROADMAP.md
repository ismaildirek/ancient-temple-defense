# Ancient Temple Defense - Asset Roadmap

## Integrated in the first environment pass

- `Generated/Environment/ward_seal_core_v1.png`: protected three-node temple ward; ember palette separates it from enemy magic.
- `Generated/Environment/enemy_shadow_breach_v2.png`: charcoal temple-stone enemy breach with restrained ember cracks and a black/burgundy shadow core; mirrored at both arena edges and pulsed subtly whenever an enemy spawns.
- `Generated/Environment/arena_foreground_ruins_v1.png`: continuous cracked masonry foreground and combat platform with clear center lane.

All three images are original built-in ImageGen outputs, stored as versioned RGBA PNGs. Unity imports them as single sprites with Point filtering, no mipmaps, alpha transparency, 100 pixels per unit, Clamp wrapping and uncompressed texture data.

## Next production groups

1. Temple state variants: intact, one seal broken, two seals broken, corrupted and destroyed.
2. Combat VFX: sword arcs, parry spark, heavy impact, ultimate rune burst and enemy dissolve strips.
3. HUD: three-seal status frame, wave counter, ultimate cooldown ring and compact control icons.
4. Enemy presentation: spawn silhouettes, elite color variants and four boss-scale enemies.
5. Environment variety: dead forest segment, flooded catacomb segment and inner-sanctum boss arena.
6. Props: braziers, chains, banners, broken statues, bone piles and destructible shrine fragments.

## Visual rules

- Dark-fantasy 32-bit pixel art with crisp Point-filtered clusters.
- World-space assets use charcoal, muted violet and weathered brown-gray stone.
- Temple/guardian power uses ember orange and deep crimson.
- Enemy/void power uses cold cyan, blue and violet.
- Keep the central combat lane readable and avoid effects that hide character silhouettes.
- Imported Asset Store folders remain untouched; first-party/generated work stays under `Assets/AncientTempleDefense`.
