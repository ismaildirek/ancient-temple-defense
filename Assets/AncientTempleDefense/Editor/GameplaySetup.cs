using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AncientTempleDefense.CameraSystem;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.Scene;
using AncientTempleDefense.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class GameplaySetup
    {
        private const string ProjectRoot = "Assets/AncientTempleDefense";
        private const string GeneratedRoot = ProjectRoot + "/Generated";
        private const string PlayerAnimationFolder = "Assets/Characters_assets/2D Pixel Art Black Knight/Animations/Black_Knight";
        private const string MonsterSpriteRoot = "Assets/Characters_assets/Monsters Creatures Fantasy/Sprites";
        private const string MapScenePath = "Assets/Scenes/Map.unity";

        private const string PlayerControllerPath = GeneratedRoot + "/Animators/BlackKnightPlayer.controller";
        private const string PlayerPrefabPath = GeneratedRoot + "/Prefabs/BlackKnightPlayer.prefab";

        [MenuItem("Tools/Ancient Temple Defense/Build Gameplay")]
        public static void BuildGameplay()
        {
            EnsureFolders();

            AnimationClip[] playerClips = LoadBlackKnightClips();
            AnimatorController playerController = CreateAnimatorController(
                PlayerControllerPath,
                playerClips,
                "BK_weapon_idle");
            GameObject playerPrefab = CreatePlayerPrefab(playerController, playerClips);

            List<GameObject> enemyPrefabs = new()
            {
                CreateEnemyAssets(new EnemyDefinition(
                    "Skeleton",
                    "Skeleton/Idle.png",
                    "Skeleton/Walk.png",
                    "Skeleton/Attack1.png",
                    "Skeleton/Attack2.png",
                    "Skeleton/Take Hit.png",
                    "Skeleton/Death.png",
                    "Skeleton/Shield.png")),
                CreateEnemyAssets(new EnemyDefinition(
                    "Goblin",
                    "Goblin/Idle.png",
                    "Goblin/Run.png",
                    "Goblin/Attack1.png",
                    "Goblin/Attack2.png",
                    "Goblin/Take Hit.png",
                    "Goblin/Death.png")),
                CreateEnemyAssets(new EnemyDefinition(
                    "Mushroom",
                    "Mushroom/Idle.png",
                    "Mushroom/Run.png",
                    "Mushroom/Attack1.png",
                    "Mushroom/Attack2.png",
                    "Mushroom/Take Hit.png",
                    "Mushroom/Death.png")),
                CreateEnemyAssets(new EnemyDefinition(
                    "FlyingEye",
                    "Flying eye/Flight.png",
                    "Flying eye/Flight.png",
                    "Flying eye/Attack1.png",
                    "Flying eye/Attack2.png",
                    "Flying eye/Take Hit.png",
                    "Flying eye/Death.png"))
            };

            ConfigureMapScene(playerPrefab, enemyPrefabs);
            LateGameEnemySetup.ConfigureAssetsAndScene();
            WaveProgressionSetup.Configure();
            GameplayEnvironmentSetup.Configure();
            GameplayAudioSetup.Configure();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Ancient Temple Defense gameplay kurulumu tamamlandi.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder(ProjectRoot, "Generated");
            EnsureFolder(GeneratedRoot, "Animations");
            EnsureFolder(GeneratedRoot + "/Animations", "Enemies");
            EnsureFolder(GeneratedRoot, "Animators");
            EnsureFolder(GeneratedRoot, "Prefabs");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static AnimationClip[] LoadBlackKnightClips()
        {
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { PlayerAnimationFolder });
            AnimationClip[] clips = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
                .Select(AssetDatabase.LoadAssetAtPath<AnimationClip>)
                .Where(clip => clip != null)
                .OrderBy(clip => clip.name, StringComparer.Ordinal)
                .ToArray();

            if (clips.Length < 30)
            {
                throw new InvalidOperationException($"Black Knight animasyonlari eksik. Bulunan klip: {clips.Length}");
            }

            return clips;
        }

        private static AnimatorController CreateAnimatorController(
            string outputPath,
            IReadOnlyCollection<AnimationClip> clips,
            string defaultStateName)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(outputPath) != null)
            {
                AssetDatabase.DeleteAsset(outputPath);
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(outputPath);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState defaultState = null;
            int index = 0;

            foreach (AnimationClip clip in clips)
            {
                AnimatorState state = stateMachine.AddState(
                    clip.name,
                    new Vector3((index % 5) * 250f, (index / 5) * 75f, 0f));
                state.motion = clip;
                state.writeDefaultValues = true;
                if (clip.name == defaultStateName)
                {
                    defaultState = state;
                }

                index++;
            }

            stateMachine.defaultState = defaultState
                ?? throw new InvalidOperationException($"Varsayilan animasyon bulunamadi: {defaultStateName}");
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static GameObject CreatePlayerPrefab(
            RuntimeAnimatorController controller,
            IReadOnlyCollection<AnimationClip> clips)
        {
            GameObject root = new("BlackKnightPlayer");
            root.transform.localScale = Vector3.one * 3.3f;

            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = FirstSprite(clips.First(clip => clip.name == "BK_weapon_idle"));
            renderer.sortingOrder = 10;

            Animator animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 5f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CapsuleCollider2D collider = root.AddComponent<CapsuleCollider2D>();
            collider.direction = CapsuleDirection2D.Vertical;
            collider.size = new Vector2(0.28f, 0.55f);
            collider.offset = new Vector2(0f, -0.015f);

            root.AddComponent<BlackKnightPlayerController>();
            root.AddComponent<PlayerHealth>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateEnemyAssets(EnemyDefinition definition)
        {
            string animationFolder = GeneratedRoot + "/Animations/Enemies/" + definition.Name;
            if (!AssetDatabase.IsValidFolder(animationFolder))
            {
                AssetDatabase.CreateFolder(GeneratedRoot + "/Animations/Enemies", definition.Name);
            }

            List<AnimationClip> clips = new()
            {
                CreateSpriteAnimation(definition.IdleTexture, animationFolder + "/Idle.anim", "Idle", true),
                CreateSpriteAnimation(definition.MoveTexture, animationFolder + "/Move.anim", "Move", true),
                CreateSpriteAnimation(definition.AttackOneTexture, animationFolder + "/Attack1.anim", "Attack1", false),
                CreateSpriteAnimation(definition.AttackTwoTexture, animationFolder + "/Attack2.anim", "Attack2", false),
                CreateSpriteAnimation(definition.HitTexture, animationFolder + "/Hit.anim", "Hit", false),
                CreateSpriteAnimation(definition.DeathTexture, animationFolder + "/Death.anim", "Death", false)
            };

            if (!string.IsNullOrEmpty(definition.DefenseTexture))
            {
                clips.Add(CreateSpriteAnimation(
                    definition.DefenseTexture,
                    animationFolder + "/Shield.anim",
                    "Shield",
                    true));
            }

            string controllerPath = GeneratedRoot + "/Animators/" + definition.Name + ".controller";
            AnimatorController controller = CreateAnimatorController(controllerPath, clips, "Idle");
            return CreateEnemyPrefab(definition, controller, clips[0]);
        }

        private static AnimationClip CreateSpriteAnimation(
            string relativeTexturePath,
            string outputPath,
            string stateName,
            bool loop)
        {
            string texturePath = MonsterSpriteRoot + "/" + relativeTexturePath;
            List<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .OrderBy(sprite => SpriteIndex(sprite.name))
                .ToList();

            if (sprites.Count == 0)
            {
                throw new InvalidOperationException($"Sprite kareleri bulunamadi: {texturePath}");
            }

            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(outputPath) != null)
            {
                AssetDatabase.DeleteAsset(outputPath);
            }

            const float frameRate = 12f;
            AnimationClip clip = new() { name = stateName, frameRate = frameRate };
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count + 1];
            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / frameRate,
                    value = sprites[i]
                };
            }

            keyframes[^1] = new ObjectReferenceKeyframe
            {
                time = sprites.Count / frameRate,
                value = loop ? sprites[0] : sprites[^1]
            };

            EditorCurveBinding binding = new()
            {
                path = string.Empty,
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite"
            };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, outputPath);
            return clip;
        }

        private static int SpriteIndex(string spriteName)
        {
            int underscore = spriteName.LastIndexOf('_');
            return underscore >= 0 && int.TryParse(spriteName[(underscore + 1)..], out int value)
                ? value
                : 0;
        }

        private static GameObject CreateEnemyPrefab(
            EnemyDefinition definition,
            RuntimeAnimatorController controller,
            AnimationClip idleClip)
        {
            GameObject root = new(definition.Name + "Enemy");
            root.transform.localScale = Vector3.one * 2.5f;

            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = FirstSprite(idleClip);
            renderer.sortingOrder = 8;

            Animator animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            CapsuleCollider2D collider = root.AddComponent<CapsuleCollider2D>();
            collider.direction = CapsuleDirection2D.Vertical;
            collider.size = definition.Name == "FlyingEye"
                ? new Vector2(0.42f, 0.42f)
                : new Vector2(0.34f, 0.62f);

            EnemyCombatant combatant = root.AddComponent<EnemyCombatant>();
            EnemyBrain brain = root.AddComponent<EnemyBrain>();

            SerializedObject brainObject = new(brain);
            brainObject.FindProperty("savunmaAnimasyonu").stringValue = definition.HasDefense ? "Shield" : string.Empty;
            brainObject.FindProperty("hareketHızı").floatValue = definition.Name == "FlyingEye" ? 2.25f : 1.8f;
            brainObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject combatantObject = new(combatant);
            combatantObject.FindProperty("gerekliVuruşSayısı").intValue = 3;
            combatantObject.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = GeneratedRoot + "/Prefabs/" + definition.Name + "Enemy.prefab";
GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static Sprite FirstSprite(AnimationClip clip)
        {
            EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(clip)
                .First(curveBinding => curveBinding.propertyName == "m_Sprite");
            ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            return keyframes[0].value as Sprite;
        }

        private static void ConfigureMapScene(GameObject playerPrefab, IReadOnlyList<GameObject> enemyPrefabs)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MapScenePath, OpenSceneMode.Single);
            GameObject existingRoot = GameObject.Find("Gameplay");
            if (existingRoot != null)
            {
                if (existingRoot.GetComponent<GameplaySceneMarker>() == null)
                {
                    throw new InvalidOperationException("Map sahnesinde Gameplay adinda kullanici nesnesi var; otomatik kurulum guvenle devam edemiyor.");
                }

                UnityEngine.Object.DestroyImmediate(existingRoot);
            }

            GameObject gameplayRoot = new("Gameplay");
            gameplayRoot.AddComponent<GameplaySceneMarker>();

            GameObject floor = new("ArenaFloor");
            floor.transform.SetParent(gameplayRoot.transform);
            floor.transform.position = new Vector3(0.5f, -4.18f, 0f);
            BoxCollider2D floorCollider = floor.AddComponent<BoxCollider2D>();
            floorCollider.size = new Vector2(39f, 0.45f);

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.name = "BlackKnightPlayer";
            player.transform.SetParent(gameplayRoot.transform);
            player.transform.position = new Vector3(-3f, -3.25f, 0f);

            GameObject spawnerObject = new("EnemyWaveSpawner");
            spawnerObject.transform.SetParent(gameplayRoot.transform);
            EnemyWaveSpawner spawner = spawnerObject.AddComponent<EnemyWaveSpawner>();
            SerializedObject spawnerSerialized = new(spawner);
            SerializedProperty prefabArray = spawnerSerialized.FindProperty("düşmanPrefabları");
            prefabArray.arraySize = enemyPrefabs.Count;
            for (int i = 0; i < enemyPrefabs.Count; i++)
            {
                prefabArray.GetArrayElementAtIndex(i).objectReferenceValue = enemyPrefabs[i].GetComponent<EnemyCombatant>();
            }

            spawnerSerialized.FindProperty("oyuncuHedefi").objectReferenceValue = player.transform;
            spawnerSerialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject overlay = new("ControlsOverlay");
overlay.transform.SetParent(gameplayRoot.transform);
            overlay.AddComponent<GameInstructionsOverlay>();

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                throw new InvalidOperationException("Map sahnesinde Main Camera bulunamadi.");
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5.4f;
            SideScrollCameraFollow cameraFollow = mainCamera.GetComponent<SideScrollCameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<SideScrollCameraFollow>();
            }

            SerializedObject cameraSerialized = new(cameraFollow);
            cameraSerialized.FindProperty("hedef").objectReferenceValue = player.transform;
            cameraSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
if (!EditorSceneManager.SaveScene(scene, MapScenePath))
            {
                throw new InvalidOperationException("Map sahnesi kaydedilemedi.");
            }
        }

        private readonly struct EnemyDefinition
        {
            public EnemyDefinition(
                string name,
                string idleTexture,
                string moveTexture,
                string attackOneTexture,
                string attackTwoTexture,
                string hitTexture,
                string deathTexture,
                string defenseTexture = "")
            {
                Name = name;
                IdleTexture = idleTexture;
                MoveTexture = moveTexture;
                AttackOneTexture = attackOneTexture;
                AttackTwoTexture = attackTwoTexture;
                HitTexture = hitTexture;
                DeathTexture = deathTexture;
                DefenseTexture = defenseTexture;
            }

            public string Name { get; }
            public string IdleTexture { get; }
            public string MoveTexture { get; }
            public string AttackOneTexture { get; }
            public string AttackTwoTexture { get; }
            public string HitTexture { get; }
            public string DeathTexture { get; }
            public string DefenseTexture { get; }
            public bool HasDefense => !string.IsNullOrEmpty(DefenseTexture);
        }
    }
}
