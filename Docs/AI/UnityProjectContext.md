# Unity Project Context

<!-- unity-onboarding:generated:start -->

## Project Summary

- Project root: `C:\Users\admin\My project`
- Last analyzed: 2026-08-12
- Last analyzed commit: unavailable; the workspace is not a Git worktree.
- Current game direction: single-player dark-fantasy 2D temple-defense fighter.

## Confirmed Environment

- Unity version: 6000.3.20f1 (`c9ba695d4f07`).
- Render pipeline: Universal Render Pipeline 17.3.0 using the project `UniversalRP` asset and 2D Renderer.
- Input system: Input System 1.19.0; Player Settings `activeInputHandler: 1` and `Assets/InputSystem_Actions.inputactions` is registered in Build Settings.
- Target platforms: Standalone is configured by imported assets; no release target is documented.

## Important Packages And Frameworks

| Area | Finding | Confidence | Evidence |
| --- | --- | --- | --- |
| 2D | Unity 2D Animation, Sprite, Tilemap, SpriteShape and Aseprite packages are installed. | Confirmed | `Packages/manifest.json` |
| Rendering | URP 17.3.0 with a 2D renderer asset. | Confirmed | `Packages/manifest.json`, `ProjectSettings/QualitySettings.asset`, `Assets/Settings/Renderer2D.asset` |
| Input | Input System 1.19.0 with a `Player` action map and keyboard WASD bindings. | Confirmed | `Packages/manifest.json`, `Assets/InputSystem_Actions.inputactions` |
| Testing | Unity Test Framework 1.6.0 is installed, but no project test assemblies or test scripts exist. | Confirmed | `Packages/manifest.json`, asset search |
| Networking | No first-party multiplayer implementation was found. | Confirmed | source and assembly search |

## Directory Structure

| Path | Purpose | Confidence | Evidence |
| --- | --- | --- | --- |
| `Assets/Scenes` | First-party gameplay scene; currently `Map.unity`. | Confirmed | Build Settings and scene YAML |
| `Assets/AncientTempleDefense` | First-party environment art and the appropriate root for new game code/assets. | Confirmed | asset contents |
| `Assets/2D Pixel Art Black Knight` | Imported player-character sprites, animation clips, sample controllers and prefabs. | Confirmed | package contents and license/readme |
| `Assets/Monsters Creatures Fantasy` | Imported Skeleton, Goblin, Mushroom and Flying Eye sprite animations plus demo assets. | Confirmed | package contents |
| `Assets/Martial Hero` | Imported alternate character package, currently unused by the requested feature. | Confirmed | asset contents |
| `Assets/Blackthornprod` | Imported fantasy-character package, currently unused by the requested feature. | Confirmed | asset contents |

## Assembly Boundaries

- No project `.asmdef`, `.asmref`, or first-party `.cs` files existed at analysis time.
- Runtime code therefore compiles into the default `Assembly-CSharp`; editor tooling belongs under an `Editor` folder in `Assembly-CSharp-Editor` unless an assembly definition is added later.

## Scenes And Startup Flow

- Build scenes: `Assets/Scenes/Map.unity` is the only enabled scene.
- Likely startup scene: `Assets/Scenes/Map.unity`.
- Scene loading flow: no scene-loading code exists.
- Current Map roots: Main Camera, Global Light 2D, 3840x1080 valley background, and transparent ancient-temple sprite.

## Architecture

| Pattern | Finding | Confidence | Evidence |
| --- | --- | --- | --- |
| Scene composition | A single serialized Map scene owns the current visual composition. | Confirmed | `Assets/Scenes/Map.unity` |
| Gameplay architecture | None yet; imported packages contain presentation assets rather than first-party gameplay logic. | Confirmed | source search and package inspection |
| Animation | Imported sprites are frame-sliced and accompanied by individual animation clips/controllers. | Confirmed | imported `.anim`, `.controller`, prefab and sprite metadata |

## Coding Conventions

- Namespace style: unknown; no first-party code exists.
- Serialized fields: unknown; use `[SerializeField] private` and safe defaults for new code.
- Async: not used.
- Comments/docs: keep intent-focused comments and update `Assets/AncientTempleDefense/README.md` for player-facing setup.

## Testing And Validation

- EditMode tests: none.
- PlayMode tests: none.
- CI/build validation: none found.
- Local Unity Editor available at `C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe` for batch-mode import/compile/build validation.

## Available Unity Tooling

| Capability | Status | Evidence |
| --- | --- | --- |
| Connected Unity MCP capabilities | unavailable | no Unity MCP tools exposed in the current session |
| Local Unity Editor | available | Unity Hub installation discovery |
| Console/read/build/test through batch mode | available | matching Editor executable installed |
| Interactive visual Play Mode inspection | unverified | no connected Editor automation provider |

## Important Constraints

- Preserve imported Asset Store package contents; place first-party additions under `Assets/AncientTempleDefense`.
- Use Point-filtered pixel-art presentation and the URP 2D renderer.
- Avoid unnecessary package or Project Settings changes.
- Scene/prefab/controller edits require Unity import and serialized-reference validation.

## Unknowns And Confidence

- Intended final platform, controller/gamepad bindings, audio, temple health, wave progression, and camera-follow behavior are not yet specified.
- No current gameplay baseline exists, so new feature behavior must be validated from generated prefabs and the Map scene.

## Source Files Inspected

- `ProjectSettings/ProjectVersion.txt`
- `ProjectSettings/EditorBuildSettings.asset`
- `ProjectSettings/GraphicsSettings.asset`
- `ProjectSettings/QualitySettings.asset`
- `ProjectSettings/ProjectSettings.asset`
- `Packages/manifest.json`
- `Assets/InputSystem_Actions.inputactions`
- `Assets/Scenes/Map.unity`
- representative Black Knight prefabs, clips, controllers, sprite metadata, and readme
- monster demo scene, controllers, clips, and sprite metadata
- `Assets/AncientTempleDefense/README.md`

<!-- unity-onboarding:generated:end -->
