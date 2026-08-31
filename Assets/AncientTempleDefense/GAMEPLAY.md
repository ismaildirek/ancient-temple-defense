# Ancient Temple Defense - Gameplay

## Controls

| Key | Action |
| --- | --- |
| `A` / `D` | Move left / right |
| `W` | Jump |
| `S` | Roll |
| `1` | Light combo; cycles through all four Black Knight attack clips |
| `2` | Heavy combo; cycles through all three heavy-attack clips |
| `3` | Parry / defense sequence |
| `4` | Weapon buff followed by the art/ultimate; gives a temporary movement bonus and a wider hit area |
| `Q` | Sheathe / draw the weapon |

Arrow keys mirror the movement, jump, and roll controls. Numpad `1-4` mirror the combat controls.

## Player

- Prefab: `Generated/Prefabs/BlackKnightPlayer.prefab`
- Animator: `Generated/Animators/BlackKnightPlayer.controller`
- All 34 animation clips from `Assets/Characters_assets/2D Pixel Art Black Knight/Animations/Black_Knight` are included in the controller.
- Gameplay actively uses the weapon/unarmed idle, run, jump, midair, fall, land and roll variants; four attacks; three heavy attacks; three parry clips; weapon on/off; buff; and weapon art.
- Hurt and death clips are retained in the controller for a future player-health system.
- The player intentionally has no health or damage receiver in this version.

## Enemies

Generated prefabs:

- `Generated/Prefabs/SkeletonEnemy.prefab`
- `Generated/Prefabs/GoblinEnemy.prefab`
- `Generated/Prefabs/MushroomEnemy.prefab`
- `Generated/Prefabs/FlyingEyeEnemy.prefab`

Every enemy has Idle/Move, Attack1, Attack2, Hit and Death animations generated directly from the original package spritesheets. Skeleton also uses Shield. Flying Eye uses Flight frames for its Idle and Move states.

Every enemy requires exactly three successful player hits. Hit one and two play the Hit animation; hit three disables combat, plays Death and despawns the enemy after the clip completes. Enemy attacks are currently presentation-only because the player has no health.

`EnemyWaveSpawner` starts with one of every enemy type and continues spawning alternately from the left and right, up to eight living enemies.

## Audio

- Black Knight uses only `Assets/Musics/SwordSoundPack`: separate randomized sets cover light attacks, heavy attacks, parry, weapon art, draw and sheathe actions.
- Each selected sword WAV stores its measured transient-peak time. Playback is delayed or starts from an offset so that the strongest part lands on the animation contact frame (42% for normal attacks and 58% for the weapon art); excessive tails are stopped after the action.
- Enemy prefabs use only the Leohpaz battle effects. Skeleton uses slash/block/death sounds, Goblin uses slash/claw/flesh/death sounds, and Mushroom/Flying Eye use claw/bite/flesh/death sounds.
- `Battle Theme 1_demo.wav` starts with the Map scene, loops at 24% volume and persists across later scene changes without creating duplicate music players.
- Combat effects are preloaded PCM clips for accurate seeking. The battle theme uses background loading and streaming Vorbis import settings.

## Scene Integration

- Startup/build scene: `Assets/Scenes/Map.unity`
- Generated scene root: `Gameplay`
- Player starts left of the temple.
- An invisible static arena floor supports jumping and landing.
- The camera follows the player horizontally while remaining inside the 3840-pixel background bounds.
- A small on-screen control legend is shown at runtime.

The setup is reproducible from Unity with `Tools > Ancient Temple Defense > Build Gameplay`. The tool only recreates assets under `Assets/AncientTempleDefense/Generated` and the marked `Gameplay` scene root; imported Asset Store folders remain unchanged.

## Validation

- EditMode: 17 tests covering three-hit health, complete Animator sets, prefabs, Map scene wiring, audio-package separation and import settings.
- PlayMode: 3 tests covering scene startup, all four enemy spawns, three-hit death/despawn, real `D` movement, the enabled Digit1 light-attack binding, persistent music and runtime combat-audio selection.
- Test artifacts: project-root `TestResults/`.
