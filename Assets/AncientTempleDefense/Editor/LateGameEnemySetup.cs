using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AncientTempleDefense.Enemies;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class LateGameEnemySetup
    {
        private const string GeneratedRoot = "Assets/AncientTempleDefense/Generated";
        private const string GeneratedAnimationRoot = GeneratedRoot + "/Animations/Enemies";
        private const string GeneratedAnimatorRoot = GeneratedRoot + "/Animators";
        private const string GeneratedPrefabRoot = GeneratedRoot + "/Prefabs";
        private const string NewEnemyRoot = "Assets/Characters_assets/new_enemy/Sprites";
        private const string BossOneRoot = "Assets/Characters_assets/BOSS_1/Animation";
        private const string BossTwoRoot = "Assets/Characters_assets/BOSS_2";
        private const string MapScenePath = "Assets/Scenes/Map.unity";

        private static readonly NewEnemyDefinition[] NewEnemies =
        {
            new("NewEnemy1", "Enemy1", "idle", "walk", "attack-A", "attack-B", "", "hit", "dead", "", "jump", 1.85f, 2.5f),
            new("NewEnemy2", "Enemy2", "idle", "walk", "attack-A", "attack-B", "", "hit", "dead", "shield-block", "jump", 1.65f, 2.5f),
            new("NewEnemy3", "Enemy3", "idle", "walk", "attack-A", "attack-B", "attack-C", "hit", "dead", "", "", 1.75f, 2.65f),
            new("NewEnemy4", "Enemy4", "idle", "walk", "attack-A", "attack-B", "", "hit", "dead", "", "jump", 2.0f, 2.5f),
            new("NewEnemy5", "Enemy5", "idle", "run", "attack-A", "attack-B", "", "hit", "dead", "", "jump", 2.15f, 2.55f)
        };

        [MenuItem("Tools/Ancient Temple Defense/Configure Late Game Enemies And Bosses")]
        public static void Configure()
        {
            ConfigureAssetsAndScene();
            GameplayAudioSetup.Configure();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Geç dönem düşmanları ile wave 7 ve 12 boss karşılaşmaları tamamlandı.");
        }

        internal static void ConfigureAssetsAndScene()
        {
            EnsureFolders();
            LateGameEnemyGeometrySetup.ConfigureSpritePivots();

            List<GameObject> lateEnemyPrefabs = NewEnemies
                .Select(CreateNewEnemyAssets)
                .ToList();
            GameObject bossOne = CreateBossOneAssets();
            GameObject bossTwo = CreateBossTwoAssets();
            LateGameEnemyAudioSetup.Configure();
            LateGameEnemyGeometrySetup.ConfigureGeneratedPrefabs();
            LateGameEnemyGeometrySetup.ConfigureSpriteAtlas();
            ConfigureMapScene(lateEnemyPrefabs, bossOne, bossTwo);
        }

        private static void EnsureFolders()
        {
            EnsureFolder(GeneratedRoot, "Animations");
            EnsureFolder(GeneratedRoot + "/Animations", "Enemies");
            EnsureFolder(GeneratedRoot, "Animators");
            EnsureFolder(GeneratedRoot, "Prefabs");

            foreach (NewEnemyDefinition definition in NewEnemies)
            {
                EnsureFolder(GeneratedAnimationRoot, definition.Name);
            }

            EnsureFolder(GeneratedAnimationRoot, "Boss1");
            EnsureFolder(GeneratedAnimationRoot, "Boss2");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static GameObject CreateNewEnemyAssets(NewEnemyDefinition definition)
        {
            string sourceFolder = NewEnemyRoot + "/" + definition.SourceFolder;
            string outputFolder = GeneratedAnimationRoot + "/" + definition.Name;
            List<AnimationClip> clips = new()
            {
                CreateIndividualFrameAnimation(sourceFolder, definition.IdlePrefix, outputFolder + "/Idle.anim", "Idle", true),
                CreateIndividualFrameAnimation(sourceFolder, definition.MovePrefix, outputFolder + "/Move.anim", "Move", true),
                CreateIndividualFrameAnimation(sourceFolder, definition.AttackOnePrefix, outputFolder + "/Attack1.anim", "Attack1", false),
                CreateIndividualFrameAnimation(sourceFolder, definition.AttackTwoPrefix, outputFolder + "/Attack2.anim", "Attack2", false),
                CreateIndividualFrameAnimation(sourceFolder, definition.HitPrefix, outputFolder + "/Hit.anim", "Hit", false),
                CreateIndividualFrameAnimation(sourceFolder, definition.DeathPrefix, outputFolder + "/Death.anim", "Death", false)
            };

            if (!string.IsNullOrEmpty(definition.AttackThreePrefix))
            {
                clips.Add(CreateIndividualFrameAnimation(
                    sourceFolder,
                    definition.AttackThreePrefix,
                    outputFolder + "/Attack3.anim",
                    "Attack3",
                    false));
            }

            if (!string.IsNullOrEmpty(definition.DefensePrefix))
            {
                clips.Add(CreateIndividualFrameAnimation(
                    sourceFolder,
                    definition.DefensePrefix,
                    outputFolder + "/Shield.anim",
                    "Shield",
                    true));
            }

            if (!string.IsNullOrEmpty(definition.JumpPrefix))
            {
                clips.Add(CreateIndividualFrameAnimation(
                    sourceFolder,
                    definition.JumpPrefix,
                    outputFolder + "/Jump.anim",
                    "Jump",
                    false));
            }

            AnimatorController controller = CreateAnimatorController(
                GeneratedAnimatorRoot + "/" + definition.Name + ".controller",
                clips,
                "Idle");
            return CreateNewEnemyPrefab(definition, controller, clips[0]);
        }

        private static AnimationClip CreateIndividualFrameAnimation(
            string sourceFolder,
            string filenamePrefix,
            string outputPath,
            string stateName,
            bool loop)
        {
            List<Sprite> sprites = AssetDatabase.FindAssets("t:Sprite", new[] { sourceFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => string.Equals(Path.GetDirectoryName(path)?.Replace('\\', '/'), sourceFolder, StringComparison.Ordinal))
                .Where(path => Path.GetFileNameWithoutExtension(path)
                    .StartsWith(filenamePrefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(TrailingNumber)
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .Where(sprite => sprite != null)
                .ToList();

            return SaveSpriteAnimation(sprites, outputPath, stateName, loop, 12f);
        }

        private static AnimationClip CreateSpriteSheetAnimation(
            string texturePath,
            string outputPath,
            string stateName,
            bool loop,
            float frameRate = 12f)
        {
            List<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .OrderBy(sprite => TrailingNumber(sprite.name))
                .ToList();
            return SaveSpriteAnimation(sprites, outputPath, stateName, loop, frameRate);
        }

        private static AnimationClip SaveSpriteAnimation(
            IReadOnlyList<Sprite> sprites,
            string outputPath,
            string stateName,
            bool loop,
            float frameRate)
        {
            if (sprites.Count == 0)
            {
                throw new InvalidOperationException($"Animasyon kareleri bulunamadı: {stateName}");
            }

            AnimationClip generated = new() { name = stateName, frameRate = frameRate };
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count + 1];
            for (int index = 0; index < sprites.Count; index++)
            {
                keyframes[index] = new ObjectReferenceKeyframe
                {
                    time = index / frameRate,
                    value = sprites[index]
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
            AnimationUtility.SetObjectReferenceCurve(generated, binding, keyframes);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(generated);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(generated, settings);

            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(outputPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(generated, outputPath);
                return generated;
            }

            EditorUtility.CopySerialized(generated, existing);
            existing.name = stateName;
            EditorUtility.SetDirty(existing);
            UnityEngine.Object.DestroyImmediate(generated);
            return existing;
        }

        private static AnimatorController CreateAnimatorController(
            string outputPath,
            IReadOnlyCollection<AnimationClip> clips,
            string defaultStateName)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(outputPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(outputPath);
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            foreach (ChildAnimatorState childState in stateMachine.states.ToArray())
            {
                stateMachine.RemoveState(childState.state);
            }

            AnimatorState defaultState = null;
            int index = 0;
            foreach (AnimationClip clip in clips.Where(clip => clip != null))
            {
                AnimatorState state = stateMachine.AddState(
                    clip.name,
                    new Vector3((index % 4) * 260f, (index / 4) * 90f, 0f));
                state.motion = clip;
                state.writeDefaultValues = true;
                if (clip.name == defaultStateName)
                {
                    defaultState = state;
                }

                index++;
            }

            stateMachine.defaultState = defaultState
                ?? throw new InvalidOperationException($"Varsayılan animasyon bulunamadı: {defaultStateName}");
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static GameObject CreateNewEnemyPrefab(
            NewEnemyDefinition definition,
            RuntimeAnimatorController controller,
            AnimationClip idleClip)
        {
            GameObject root = CreateActorRoot(definition.Name, controller, idleClip, definition.Scale, new Vector2(0.36f, 0.66f));
            EnemyCombatant combatant = root.AddComponent<EnemyCombatant>();
            EnemyBrain brain = root.AddComponent<EnemyBrain>();

            SerializedObject brainObject = new(brain);
            brainObject.FindProperty("üçüncüSaldırıAnimasyonu").stringValue =
                string.IsNullOrEmpty(definition.AttackThreePrefix) ? string.Empty : "Attack3";
            brainObject.FindProperty("savunmaAnimasyonu").stringValue =
                string.IsNullOrEmpty(definition.DefensePrefix) ? string.Empty : "Shield";
            brainObject.FindProperty("hareketHızı").floatValue = definition.MoveSpeed;
            brainObject.FindProperty("saldırıMenzili").floatValue = 1.5f;
            brainObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject combatantObject = new(combatant);
            combatantObject.FindProperty("gerekliVuruşSayısı").intValue = 3;
            combatantObject.FindProperty("doğmaYOfseti").floatValue = 0f;
            combatantObject.ApplyModifiedPropertiesWithoutUndo();

            return SavePrefab(root, GeneratedPrefabRoot + "/" + definition.Name + ".prefab");
        }

        private static GameObject CreateBossOneAssets()
        {
            AnimationClip[] clips =
            {
                LoadClip(BossOneRoot + "/Idle.anim"),
                LoadClip(BossOneRoot + "/Walk.anim"),
                LoadClip(BossOneRoot + "/Attack.anim"),
                LoadClip(BossOneRoot + "/Cast.anim"),
                LoadClip(BossOneRoot + "/Spell.anim"),
                LoadClip(BossOneRoot + "/Hurt.anim"),
                LoadClip(BossOneRoot + "/Death.anim"),
                LoadClip(BossOneRoot + "/Attack-NoEffect.anim"),
                LoadClip(BossOneRoot + "/Cast-NoEffect.anim"),
                LoadClip(BossOneRoot + "/Spell-NoEffect.anim"),
                LoadClip(BossOneRoot + "/Hurt-NoEffect.anim"),
                LoadClip(BossOneRoot + "/Death-NoEffect.anim")
            };
            AnimatorController controller = CreateAnimatorController(
                GeneratedAnimatorRoot + "/Boss1.controller",
                clips,
                "Idle");
            GameObject root = CreateActorRoot("Boss1Enemy", controller, clips[0], 6.0f, new Vector2(0.5f, 0.82f));
            ConfigureBossComponents(root, 1, "Walk", "Attack", "Cast", string.Empty, "Spell", "Hurt", 20, 1.35f, 1.65f, 2.4f, 5f);
            return SavePrefab(root, GeneratedPrefabRoot + "/Boss1Enemy.prefab");
        }

        private static GameObject CreateBossTwoAssets()
        {
            string outputFolder = GeneratedAnimationRoot + "/Boss2";
            AnimationClip special = CreateSpriteSheetAnimation(
                BossTwoRoot + "/Sprites/Attack2.png",
                outputFolder + "/Special2.anim",
                "Special2",
                false);
            AnimationClip hit = CreateSpriteSheetAnimation(
                BossTwoRoot + "/Sprites/Take hit.png",
                outputFolder + "/Hit.anim",
                "Hit",
                false);
            AnimationClip fall = CreateSpriteSheetAnimation(
                BossTwoRoot + "/Sprites/Fall.png",
                outputFolder + "/Fall.anim",
                "Fall",
                false);
            AnimationClip[] clips =
            {
                LoadClip(BossTwoRoot + "/Animations/Idle.anim"),
                LoadClip(BossTwoRoot + "/Animations/Run.anim"),
                LoadClip(BossTwoRoot + "/Animations/Attack1.anim"),
                LoadClip(BossTwoRoot + "/Animations/Attack2.anim"),
                LoadClip(BossTwoRoot + "/Animations/Jump.anim"),
                hit,
                fall,
                LoadClip(BossTwoRoot + "/Animations/Death.anim"),
                special
            };
            AnimatorController controller = CreateAnimatorController(
                GeneratedAnimatorRoot + "/Boss2.controller",
                clips,
                "Idle");
            GameObject root = CreateActorRoot("Boss2Enemy", controller, clips[0], 3.3f, new Vector2(0.62f, 1.45f));
            ConfigureBossComponents(root, 2, "Run", "Attack1", "Attack2", "Jump", "Special2", "Hit", 36, 1.55f, 1.85f, 3.0f, 4.2f);
            return SavePrefab(root, GeneratedPrefabRoot + "/Boss2Enemy.prefab");
        }

        private static GameObject CreateActorRoot(
            string name,
            RuntimeAnimatorController controller,
            AnimationClip idleClip,
            float scale,
            Vector2 colliderSize)
        {
            GameObject root = new(name);
            root.transform.localScale = Vector3.one * scale;

            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = FirstSprite(idleClip);
            renderer.sortingOrder = 9;

            Animator animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            CapsuleCollider2D collider = root.AddComponent<CapsuleCollider2D>();
            collider.direction = CapsuleDirection2D.Vertical;
            collider.size = colliderSize;
            return root;
        }

        private static void ConfigureBossComponents(
            GameObject root,
            int tier,
            string moveState,
            string normalAttackState,
            string heavyAttackState,
            string preparationState,
            string specialAttackState,
            string hitState,
            int baseHealth,
            float moveSpeed,
            float heavyDamageMultiplier,
            float specialDamageMultiplier,
            float specialCooldown)
        {
            EnemyCombatant combatant = root.AddComponent<EnemyCombatant>();
            BossEnemyBrain brain = root.AddComponent<BossEnemyBrain>();

            SerializedObject combatantObject = new(combatant);
            combatantObject.FindProperty("hasarAlmaAnimasyonu").stringValue = hitState;
            combatantObject.FindProperty("ölümAnimasyonu").stringValue = "Death";
            combatantObject.FindProperty("gerekliVuruşSayısı").intValue = baseHealth;
            combatantObject.FindProperty("doğmaYOfseti").floatValue = 0f;
            combatantObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject brainObject = new(brain);
            brainObject.FindProperty("bossSeviyesi").intValue = tier;
            brainObject.FindProperty("hareketAnimasyonu").stringValue = moveState;
            brainObject.FindProperty("normalSaldırıAnimasyonu").stringValue = normalAttackState;
            brainObject.FindProperty("ağırSaldırıAnimasyonu").stringValue = heavyAttackState;
            brainObject.FindProperty("özelHazırlıkAnimasyonu").stringValue = preparationState;
            brainObject.FindProperty("özelSaldırıAnimasyonu").stringValue = specialAttackState;
            brainObject.FindProperty("hareketHızı").floatValue = moveSpeed;
            brainObject.FindProperty("saldırıMenzili").floatValue = tier == 1 ? 1.85f : 2.0f;
            brainObject.FindProperty("temelSaldırıHasarı").intValue = tier == 1 ? 12 : 16;
            brainObject.FindProperty("ağırHasarÇarpanı").floatValue = heavyDamageMultiplier;
            brainObject.FindProperty("özelHasarÇarpanı").floatValue = specialDamageMultiplier;
            brainObject.FindProperty("ağırMenzilÇarpanı").floatValue = tier == 1 ? 1.4f : 1.55f;
            brainObject.FindProperty("özelMenzilÇarpanı").floatValue = tier == 1 ? 2.2f : 2.5f;
            brainObject.FindProperty("özelSaldırıBeklemeSüresi").floatValue = specialCooldown;
            brainObject.FindProperty("özelİçinGerekenSaldırı").intValue = tier == 1 ? 2 : 1;
            brainObject.FindProperty("vuruşTemasOranı").floatValue = tier == 1 ? 0.58f : 0.64f;
            brainObject.FindProperty("kaynakVarsayilanYonuSola").boolValue = tier == 1;
            brainObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject SavePrefab(GameObject root, string outputPath)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, outputPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static AnimationClip LoadClip(string path)
        {
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(path)
                ?? throw new InvalidOperationException($"Animasyon klibi bulunamadı: {path}");
        }

        private static Sprite FirstSprite(AnimationClip clip)
        {
            EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(clip)
                .FirstOrDefault(curveBinding => curveBinding.propertyName == "m_Sprite");
            ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            return keyframes.FirstOrDefault().value as Sprite
                ?? throw new InvalidOperationException($"İlk sprite bulunamadı: {clip.name}");
        }

        private static int TrailingNumber(string pathOrName)
        {
            string name = Path.GetFileNameWithoutExtension(pathOrName);
            int separator = Math.Max(name.LastIndexOf('-'), name.LastIndexOf('_'));
            return separator >= 0 && int.TryParse(name[(separator + 1)..], out int number)
                ? number
                : 0;
        }

        private static void ConfigureMapScene(
            IReadOnlyList<GameObject> lateEnemyPrefabs,
            GameObject bossOne,
            GameObject bossTwo)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MapScenePath, OpenSceneMode.Single);
            EnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EnemyWaveSpawner>()
                ?? throw new InvalidOperationException("Map sahnesinde EnemyWaveSpawner bulunamadı.");
            SerializedObject serialized = new(spawner);

            SerializedProperty lateArray = serialized.FindProperty("geçDönemDüşmanPrefabları");
            lateArray.arraySize = lateEnemyPrefabs.Count;
            for (int index = 0; index < lateEnemyPrefabs.Count; index++)
            {
                lateArray.GetArrayElementAtIndex(index).objectReferenceValue =
                    lateEnemyPrefabs[index].GetComponent<EnemyCombatant>();
            }

            serialized.FindProperty("geçDönemBaşlangıçWave").intValue = 8;
            serialized.FindProperty("birinciBossPrefabı").objectReferenceValue = bossOne.GetComponent<EnemyCombatant>();
            serialized.FindProperty("birinciBossWave").intValue = 7;
            serialized.FindProperty("birinciBossCanÇarpanı").floatValue = 4f;
            serialized.FindProperty("birinciBossHasarÇarpanı").floatValue = 1.4f;
            serialized.FindProperty("birinciBossHızÇarpanı").floatValue = 1.05f;
            serialized.FindProperty("ikinciBossPrefabı").objectReferenceValue = bossTwo.GetComponent<EnemyCombatant>();
            serialized.FindProperty("ikinciBossWave").intValue = 12;
            serialized.FindProperty("ikinciBossCanÇarpanı").floatValue = 6f;
            serialized.FindProperty("ikinciBossHasarÇarpanı").floatValue = 1.8f;
            serialized.FindProperty("ikinciBossHızÇarpanı").floatValue = 1.2f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, MapScenePath))
            {
                throw new InvalidOperationException("Geç dönem düşmanları Map sahnesine kaydedilemedi.");
            }
        }

        private readonly struct NewEnemyDefinition
        {
            public NewEnemyDefinition(
                string name,
                string sourceFolder,
                string idlePrefix,
                string movePrefix,
                string attackOnePrefix,
                string attackTwoPrefix,
                string attackThreePrefix,
                string hitPrefix,
                string deathPrefix,
                string defensePrefix,
                string jumpPrefix,
                float moveSpeed,
                float scale)
            {
                Name = name;
                SourceFolder = sourceFolder;
                IdlePrefix = idlePrefix;
                MovePrefix = movePrefix;
                AttackOnePrefix = attackOnePrefix;
                AttackTwoPrefix = attackTwoPrefix;
                AttackThreePrefix = attackThreePrefix;
                HitPrefix = hitPrefix;
                DeathPrefix = deathPrefix;
                DefensePrefix = defensePrefix;
                JumpPrefix = jumpPrefix;
                MoveSpeed = moveSpeed;
                Scale = scale;
            }

            public string Name { get; }
            public string SourceFolder { get; }
            public string IdlePrefix { get; }
            public string MovePrefix { get; }
            public string AttackOnePrefix { get; }
            public string AttackTwoPrefix { get; }
            public string AttackThreePrefix { get; }
            public string HitPrefix { get; }
            public string DeathPrefix { get; }
            public string DefensePrefix { get; }
            public string JumpPrefix { get; }
            public float MoveSpeed { get; }
            public float Scale { get; }
        }
    }
}
