using System;
using System.Collections.Generic;
using System.Linq;
using AncientTempleDefense.Audio;
using AncientTempleDefense.Economy;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Vfx;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class SpecialEnemySetup
    {
        private const string SourceRoot = "Assets/Characters_assets/new_enemy/new_4nenemy";
        private const string GeneratedAnimationRoot = "Assets/AncientTempleDefense/Generated/Animations/SpecialEnemies";
        private const string GeneratedControllerRoot = "Assets/AncientTempleDefense/Generated/Animators";
        private const string GeneratedPrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";
        private const string MapPath = "Assets/Scenes/Map.unity";
        private const string CoinPath = GeneratedPrefabRoot + "/CoinPickup.prefab";
        private const string PotionPath = GeneratedPrefabRoot + "/HealthPotionPickup.prefab";

        private const string BloodHitPath = "Assets/100BestEffectPack/Effects/BloodEffect/BloodEffect2.prefab";
        private const string BloodAttackPath = "Assets/100BestEffectPack/Effects/BloodEffect/BloodEffect2.prefab";
        private const string BloodDeathPath = "Assets/100BestEffectPack/Effects/BloodEffect/BloodEffect5.prefab";
        private const string DarkEffectPath = "Assets/100BestEffectPack/Effects/DarkEffect/DarkEffect5.prefab";
        private const string PoisonHitPath = "Assets/100BestEffectPack/Effects/PoisonEffect/PoisonEffect2.prefab";
        private const string PoisonDeathPath = "Assets/100BestEffectPack/Effects/PoisonEffect/PoisonEffect5.prefab";
        private const string ExplosionPath = "Assets/100BestEffectPack/Effects/ExplosionEffect/ExplosionEffect4.prefab";

        private const string BattleAudioRoot = "Assets/Musics/Leohpaz/RPG_Essentials_Free/10_Battle_SFX";
        private const string MagicAudioRoot = "Assets/Musics/Leohpaz/RPG_Essentials_Free/8_Atk_Magic_SFX";
        private const string BiteAudio = BattleAudioRoot + "/08_Bite_04.wav";
        private const string ClawAudio = BattleAudioRoot + "/03_Claw_03.wav";
        private const string FleshAudio = BattleAudioRoot + "/15_Impact_flesh_02.wav";
        private const string DeathAudio = BattleAudioRoot + "/69_Enemy_death_01.wav";
        private const string PoisonAudio = MagicAudioRoot + "/46_Poison_01.wav";
        private const string FireExplosionAudio = MagicAudioRoot + "/04_Fire_explosion_04_medium.wav";

        [MenuItem("Tools/Ancient Temple Defense/Yeni Özel Düşmanları Kur")]
        public static void Configure()
        {
            EnsureFolder(GeneratedAnimationRoot);
            EnsureFolder(GeneratedControllerRoot);
            EnsureFolder(GeneratedPrefabRoot);
            AssetDatabase.Refresh();

            EnemyDefinition[] definitions = CreateDefinitions();
            Dictionary<string, GameObject> prefabs = new(StringComparer.Ordinal);
            foreach (EnemyDefinition definition in definitions)
            {
                prefabs.Add(definition.Name, BuildPrefab(definition));
            }

            ConfigureMap(prefabs);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Yeni özel düşmanlar kuruldu: Bat, Mimic, Rat yağmacı, patlayan Slime ve wave 9 Dark Wolf.");
        }

        private static EnemyDefinition[] CreateDefinitions()
        {
            string spriteRoot = SourceRoot + "/Sprites";
            string wolfAnimationRoot = SourceRoot + "/wolf_enemy/DarkWolf_2d/Art/Animations";
            return new[]
            {
                new EnemyDefinition
                {
                    Name = "BatFlyingEnemy",
                    Scale = 2f,
                    TargetMode = EnemyTargetMode.DefendersOnly,
                    IsFlying = true,
                    HealthMultiplier = 0.75f,
                    DamageMultiplier = 0.80f,
                    MovementMultiplier = 1.25f,
                    AttackSpeedMultiplier = 1.20f,
                    BaseMoveSpeed = 2.15f,
                    AttackRange = 1.15f,
                    AttackCooldown = 1.05f,
                    VfxScale = 0.028f,
                    VfxYOffset = 0.08f,
                    Audio = AudioKind.Bat,
                    Clips = SheetClips(
                        spriteRoot + "/Bat/fly.png",
                        spriteRoot + "/Bat/fly.png",
                        spriteRoot + "/Bat/attack.png",
                        spriteRoot + "/Bat/attack.png",
                        spriteRoot + "/Bat/hurt.png",
                        spriteRoot + "/Bat/death.png",
                        12f)
                },
                new EnemyDefinition
                {
                    Name = "MimicEnemy",
                    Scale = 1.4f,
                    TargetMode = EnemyTargetMode.NearestThreat,
                    HealthMultiplier = 1.65f,
                    DamageMultiplier = 1.35f,
                    MovementMultiplier = 0.75f,
                    AttackSpeedMultiplier = 0.85f,
                    BaseMoveSpeed = 1.75f,
                    AttackRange = 1.25f,
                    AttackCooldown = 1.25f,
                    VfxScale = 0.032f,
                    VfxYOffset = 0.12f,
                    Audio = AudioKind.Mimic,
                    Clips = SheetClips(
                        spriteRoot + "/Mimic/idle_transformed.png",
                        spriteRoot + "/Mimic/walk.png",
                        spriteRoot + "/Mimic/attack_1.png",
                        spriteRoot + "/Mimic/attack_2.png",
                        spriteRoot + "/Mimic/hurt.png",
                        spriteRoot + "/Mimic/death.png",
                        11f)
                },
                new EnemyDefinition
                {
                    Name = "RatRaiderEnemy",
                    Scale = 2.6f,
                    TargetMode = EnemyTargetMode.TempleOnly,
                    HealthMultiplier = 0.75f,
                    DamageMultiplier = 1.15f,
                    MovementMultiplier = 1.45f,
                    AttackSpeedMultiplier = 1.10f,
                    BaseMoveSpeed = 2.10f,
                    AttackRange = 1.05f,
                    AttackCooldown = 0.95f,
                    VfxScale = 0.024f,
                    VfxYOffset = 0.06f,
                    Audio = AudioKind.Rat,
                    Clips = SheetClips(
                        spriteRoot + "/Rat/idle.png",
                        spriteRoot + "/Rat/run.png",
                        spriteRoot + "/Rat/attack_bite.png",
                        spriteRoot + "/Rat/attack_bite.png",
                        spriteRoot + "/Rat/hurt.png",
                        spriteRoot + "/Rat/rat-death.png",
                        14f)
                },
                new EnemyDefinition
                {
                    Name = "ExplodingSlimeEnemy",
                    Scale = 1.35f,
                    TargetMode = EnemyTargetMode.NearestThreat,
                    HealthMultiplier = 1f,
                    DamageMultiplier = 0.70f,
                    MovementMultiplier = 0.80f,
                    AttackSpeedMultiplier = 0.85f,
                    BaseMoveSpeed = 1.65f,
                    AttackRange = 1.15f,
                    AttackCooldown = 1.25f,
                    VfxScale = 0.026f,
                    VfxYOffset = 0.08f,
                    Audio = AudioKind.Slime,
                    ExplodesOnDeath = true,
                    Clips = SheetClips(
                        spriteRoot + "/Slime/idle.png",
                        spriteRoot + "/Slime/walk.png",
                        spriteRoot + "/Slime/attack.png",
                        spriteRoot + "/Slime/attack.png",
                        spriteRoot + "/Slime/hurt.png",
                        spriteRoot + "/Slime/death.png",
                        12f)
                },
                new EnemyDefinition
                {
                    Name = "DarkWolfEnemy",
                    Scale = 1.4f,
                    TargetMode = EnemyTargetMode.PlayerOnly,
                    HealthMultiplier = 1.20f,
                    DamageMultiplier = 1.25f,
                    MovementMultiplier = 1.50f,
                    AttackSpeedMultiplier = 1.20f,
                    BaseMoveSpeed = 2.20f,
                    AttackRange = 1.20f,
                    AttackCooldown = 0.90f,
                    VfxScale = 0.032f,
                    VfxYOffset = 0.10f,
                    Audio = AudioKind.Wolf,
                    Clips = new Dictionary<string, ClipSource>(StringComparer.Ordinal)
                    {
                        ["Idle"] = Existing(wolfAnimationRoot + "/DarkWolf_2d_Idle Animation.anim"),
                        ["Move"] = Existing(wolfAnimationRoot + "/DarkWolf_2d_Run Animation.anim"),
                        ["Attack1"] = Existing(wolfAnimationRoot + "/DarkWolf_2d_Attack Animation.anim"),
                        ["Attack2"] = Existing(wolfAnimationRoot + "/DarkWolf_2d_Attack Animation.anim"),
                        ["Hit"] = Existing(wolfAnimationRoot + "/DarkWolf_2d_Damage Animation.anim"),
                        ["Death"] = Existing(wolfAnimationRoot + "/DarkWolf_2d_Death Animation.anim")
                    }
                }
            };
        }

        private static Dictionary<string, ClipSource> SheetClips(
            string idle,
            string move,
            string attackOne,
            string attackTwo,
            string hit,
            string death,
            float frameRate)
        {
            return new Dictionary<string, ClipSource>(StringComparer.Ordinal)
            {
                ["Idle"] = Sheet(idle, frameRate, true),
                ["Move"] = Sheet(move, frameRate, true),
                ["Attack1"] = Sheet(attackOne, frameRate, false),
                ["Attack2"] = Sheet(attackTwo, frameRate, false),
                ["Hit"] = Sheet(hit, frameRate, false),
                ["Death"] = Sheet(death, frameRate, false)
            };
        }

        private static ClipSource Sheet(string path, float frameRate, bool loop)
        {
            return new ClipSource(path, frameRate, loop, false);
        }

        private static ClipSource Existing(string path)
        {
            return new ClipSource(path, 0f, false, true);
        }

        private static GameObject BuildPrefab(EnemyDefinition definition)
        {
            Dictionary<string, AnimationClip> clips = new(StringComparer.Ordinal);
            foreach (KeyValuePair<string, ClipSource> pair in definition.Clips)
            {
                clips[pair.Key] = ResolveClip(definition.Name, pair.Key, pair.Value);
            }

            AnimatorController controller = CreateController(definition.Name, clips);
            Sprite firstSprite = ResolveFirstSprite(definition.Clips["Idle"], clips["Idle"]);
            if (firstSprite == null)
            {
                throw new InvalidOperationException($"İlk sprite bulunamadı: {definition.Name}");
            }

            GameObject root = new(definition.Name);
            try
            {
                root.transform.localScale = Vector3.one * definition.Scale;
                SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
                renderer.sprite = firstSprite;
                renderer.sortingOrder = 9;

                Animator animator = root.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

                CapsuleCollider2D collider = root.AddComponent<CapsuleCollider2D>();
                collider.isTrigger = true;
                collider.direction = CapsuleDirection2D.Vertical;
                ConfigureTightCollider(collider, firstSprite, definition.Scale);

                Rigidbody2D body = root.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;
                body.gravityScale = 0f;
                body.freezeRotation = true;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;

                EnemyCombatant combatant = root.AddComponent<EnemyCombatant>();
                EnemyBrain brain = root.AddComponent<EnemyBrain>();
                EnemyRoleProfile role = root.AddComponent<EnemyRoleProfile>();
                EnemyVfxController vfx = root.AddComponent<EnemyVfxController>();
                EnemyLootDropper loot = root.AddComponent<EnemyLootDropper>();
                EnemyAudioController audio = root.AddComponent<EnemyAudioController>();
                if (definition.ExplodesOnDeath)
                {
                    root.AddComponent<ExplodingEnemy>();
                }

                ConfigureBrain(brain, definition);
                ConfigureRole(role, definition);
                ConfigureVfx(vfx, definition);
                ConfigureLoot(loot);
                ConfigureAudio(audio, definition.Audio);

                string prefabPath = $"{GeneratedPrefabRoot}/{definition.Name}.prefab";
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                if (saved == null)
                {
                    throw new InvalidOperationException($"Prefab kaydedilemedi: {prefabPath}");
                }

                return saved;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static AnimationClip ResolveClip(string enemyName, string stateName, ClipSource source)
        {
            if (source.IsExistingClip)
            {
                return AssetDatabase.LoadAssetAtPath<AnimationClip>(source.Path)
                    ?? throw new InvalidOperationException($"Animasyon klibi bulunamadı: {source.Path}");
            }

            Sprite[] sprites = LoadSprites(source.Path);
            if (sprites.Length == 0)
            {
                throw new InvalidOperationException($"Sprite bulunamadı: {source.Path}");
            }

            string clipPath = $"{GeneratedAnimationRoot}/{enemyName}_{stateName}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
            }

            clip.frameRate = source.FrameRate;
            EditorCurveBinding spriteBinding = new()
            {
                path = string.Empty,
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite"
            };
            ObjectReferenceKeyframe[] frames = new ObjectReferenceKeyframe[sprites.Length];
            for (int index = 0; index < sprites.Length; index++)
            {
                frames[index] = new ObjectReferenceKeyframe
                {
                    time = index / source.FrameRate,
                    value = sprites[index]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, frames);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = source.Loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimatorController CreateController(
            string enemyName,
            IReadOnlyDictionary<string, AnimationClip> clips)
        {
            string path = $"{GeneratedControllerRoot}/{enemyName}.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            foreach (ChildAnimatorState child in stateMachine.states.ToArray())
            {
                stateMachine.RemoveState(child.state);
            }

            AnimatorState idle = null;
            foreach (string stateName in new[] { "Idle", "Move", "Attack1", "Attack2", "Hit", "Death" })
            {
                AnimatorState state = stateMachine.AddState(stateName);
                state.motion = clips[stateName];
                state.writeDefaultValues = true;
                if (stateName == "Idle")
                {
                    idle = state;
                }
            }

            stateMachine.defaultState = idle;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static Sprite ResolveFirstSprite(ClipSource source, AnimationClip clip)
        {
            if (!source.IsExistingClip)
            {
                return LoadSprites(source.Path).FirstOrDefault();
            }

            foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                Sprite sprite = frames.Select(frame => frame.value).OfType<Sprite>().FirstOrDefault();
                if (sprite != null)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static Sprite[] LoadSprites(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderBy(sprite => TrailingNumber(sprite.name))
                .ThenBy(sprite => sprite.name, StringComparer.Ordinal)
                .ToArray();
        }

        private static int TrailingNumber(string value)
        {
            int start = value.Length;
            while (start > 0 && char.IsDigit(value[start - 1]))
            {
                start--;
            }

            return start < value.Length && int.TryParse(value.Substring(start), out int number)
                ? number
                : int.MaxValue;
        }

        private static void ConfigureTightCollider(CapsuleCollider2D collider, Sprite sprite, float characterScale)
        {
            Vector2[] vertices = sprite.vertices;
            Bounds tightBounds = vertices.Length > 0
                ? new Bounds(vertices[0], Vector3.zero)
                : sprite.bounds;
            for (int index = 1; index < vertices.Length; index++)
            {
                tightBounds.Encapsulate(vertices[index]);
            }

            float minimumLocalHitbox = 0.46f / Mathf.Max(0.01f, characterScale);
            Vector2 size = new(
                Mathf.Max(minimumLocalHitbox, tightBounds.size.x * 0.68f),
                Mathf.Max(minimumLocalHitbox, tightBounds.size.y * 0.88f));
            collider.size = size;
            collider.offset = new Vector2(
                tightBounds.center.x,
                tightBounds.min.y + size.y * 0.5f);
        }

        private static void ConfigureBrain(EnemyBrain brain, EnemyDefinition definition)
        {
            SerializedObject serialized = new(brain);
            serialized.FindProperty("hareketHızı").floatValue = definition.BaseMoveSpeed;
            serialized.FindProperty("saldırıMenzili").floatValue = definition.AttackRange;
            serialized.FindProperty("farkEtmeMenzili").floatValue = 45f;
            serialized.FindProperty("saldırıBeklemeSüresi").floatValue = definition.AttackCooldown;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureRole(EnemyRoleProfile role, EnemyDefinition definition)
        {
            SerializedObject serialized = new(role);
            serialized.FindProperty("hedefÖnceliği").enumValueIndex = (int)definition.TargetMode;
            serialized.FindProperty("uçanDüşman").boolValue = definition.IsFlying;
            serialized.FindProperty("canÇarpanı").floatValue = definition.HealthMultiplier;
            serialized.FindProperty("hasarÇarpanı").floatValue = definition.DamageMultiplier;
            serialized.FindProperty("hareketHızıÇarpanı").floatValue = definition.MovementMultiplier;
            serialized.FindProperty("vuruşHızıÇarpanı").floatValue = definition.AttackSpeedMultiplier;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureVfx(EnemyVfxController vfx, EnemyDefinition definition)
        {
            SerializedObject serialized = new(vfx);
            serialized.FindProperty("doğmaEfekti").objectReferenceValue = null;
            serialized.FindProperty("hasarEfekti").objectReferenceValue = LoadPrefab(
                definition.Audio == AudioKind.Slime ? PoisonHitPath : BloodHitPath);
            serialized.FindProperty("saldırıTemasEfekti").objectReferenceValue = LoadPrefab(
                definition.Audio == AudioKind.Slime ? PoisonHitPath : BloodAttackPath);
            string deathEffectPath = definition.Audio == AudioKind.Slime
                ? PoisonDeathPath
                : DarkEffectPath;
            serialized.FindProperty("ölümEfekti").objectReferenceValue = LoadPrefab(deathEffectPath);
            serialized.FindProperty("özelEfekt").objectReferenceValue = definition.ExplodesOnDeath
                ? LoadPrefab(ExplosionPath)
                : null;
            serialized.FindProperty("efektÖlçeği").floatValue = definition.VfxScale;
            serialized.FindProperty("efektYOfseti").floatValue = definition.VfxYOffset;
            serialized.FindProperty("efektÖmrü").floatValue = definition.ExplodesOnDeath ? 1.15f : 0.85f;
            serialized.FindProperty("particleSıralaması").intValue = 12;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void ConfigureLoot(EnemyLootDropper loot)
        {
            SerializedObject serialized = new(loot);
            serialized.FindProperty("coinDusmeIhtimali").floatValue = 0.40f;
            serialized.FindProperty("iksirDusmeIhtimali").floatValue = 0.20f;
            serialized.FindProperty("coinPrefabi").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<WorldPickup>(CoinPath);
            serialized.FindProperty("canIksiriPrefabi").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<WorldPickup>(PotionPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureAudio(EnemyAudioController audio, AudioKind kind)
        {
            SerializedObject serialized = new(audio);
            string[] attackOne;
            string[] attackTwo;
            switch (kind)
            {
                case AudioKind.Slime:
                    attackOne = new[] { PoisonAudio };
                    attackTwo = new[] { FireExplosionAudio };
                    break;
                case AudioKind.Mimic:
                    attackOne = new[] { BiteAudio };
                    attackTwo = new[] { ClawAudio };
                    break;
                case AudioKind.Bat:
                case AudioKind.Wolf:
                    attackOne = new[] { BiteAudio, ClawAudio };
                    attackTwo = new[] { ClawAudio };
                    break;
                default:
                    attackOne = new[] { BiteAudio };
                    attackTwo = new[] { BiteAudio };
                    break;
            }

            ConfigureAudioSet(serialized, "birinciSaldırıSesleri", attackOne, 0.66f, 0.08f);
            ConfigureAudioSet(serialized, "ikinciSaldırıSesleri", attackTwo, 0.68f, 0.10f);
            ConfigureAudioSet(serialized, "hasarAlmaSesleri", new[] { FleshAudio }, 0.62f, 0.04f);
            ConfigureAudioSet(serialized, "ölümSesleri", new[] { DeathAudio }, 0.70f, 0.06f);
            ConfigureAudioSet(serialized, "savunmaSesleri", Array.Empty<string>(), 0f, 0f);
            ConfigureAudioSet(serialized, "özelSaldırıSesleri", Array.Empty<string>(), 0f, 0f);
            ConfigureAudioSet(serialized, "güçlüÖzelSaldırıSesleri", Array.Empty<string>(), 0f, 0f);
            serialized.FindProperty("eşZamanlıSesSayısı").intValue = 3;
            serialized.FindProperty("üçBoyutluSesKarışımı").floatValue = 0.18f;
            serialized.FindProperty("saldırıTemasZamanı").floatValue = 0.48f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureAudioSet(
            SerializedObject owner,
            string propertyName,
            IReadOnlyList<string> paths,
            float volume,
            float peakTime)
        {
            SerializedProperty set = owner.FindProperty(propertyName);
            SerializedProperty clips = set.FindPropertyRelative("sesKlipleri");
            clips.arraySize = paths.Count;
            for (int index = 0; index < paths.Count; index++)
            {
                SerializedProperty item = clips.GetArrayElementAtIndex(index);
                item.FindPropertyRelative("sesKlibi").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<AudioClip>(paths[index]);
                item.FindPropertyRelative("vuruşZirvesiSaniyesi").floatValue = peakTime;
            }

            set.FindPropertyRelative("sesSeviyesi").floatValue = volume;
            set.FindPropertyRelative("enDüşükPerde").floatValue = 0.92f;
            set.FindPropertyRelative("enYüksekPerde").floatValue = 1.08f;
            set.FindPropertyRelative("yumuşakBitişSüresi").floatValue = 0.14f;
        }

        private static void ConfigureMap(IReadOnlyDictionary<string, GameObject> prefabs)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MapPath, OpenSceneMode.Single);
            EnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EnemyWaveSpawner>(
                FindObjectsInactive.Include);
            if (spawner == null)
            {
                throw new InvalidOperationException("Map sahnesinde EnemyWaveSpawner bulunamadı.");
            }

            SerializedObject serialized = new(spawner);
            SerializedProperty late = serialized.FindProperty("geçDönemDüşmanPrefabları");
            List<EnemyCombatant> roster = new();
            for (int index = 0; index < late.arraySize; index++)
            {
                EnemyCombatant existing = late.GetArrayElementAtIndex(index).objectReferenceValue as EnemyCombatant;
                if (existing != null && !roster.Contains(existing))
                {
                    roster.Add(existing);
                }
            }

            foreach (string name in new[] { "BatFlyingEnemy", "MimicEnemy", "RatRaiderEnemy", "ExplodingSlimeEnemy" })
            {
                EnemyCombatant candidate = prefabs[name].GetComponent<EnemyCombatant>();
                if (!roster.Contains(candidate))
                {
                    roster.Add(candidate);
                }
            }

            late.arraySize = roster.Count;
            for (int index = 0; index < roster.Count; index++)
            {
                late.GetArrayElementAtIndex(index).objectReferenceValue = roster[index];
            }

            serialized.FindProperty("kurtPrefabı").objectReferenceValue =
                prefabs["DarkWolfEnemy"].GetComponent<EnemyCombatant>();
            serialized.FindProperty("kurtBaşlangıçWave").intValue = 9;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject LoadPrefab(string path)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path)
                ?? throw new InvalidOperationException($"Efekt prefabı bulunamadı: {path}");
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        private sealed class EnemyDefinition
        {
            public string Name;
            public float Scale;
            public EnemyTargetMode TargetMode;
            public bool IsFlying;
            public float HealthMultiplier;
            public float DamageMultiplier;
            public float MovementMultiplier;
            public float AttackSpeedMultiplier;
            public float BaseMoveSpeed;
            public float AttackRange;
            public float AttackCooldown;
            public float VfxScale;
            public float VfxYOffset;
            public bool ExplodesOnDeath;
            public AudioKind Audio;
            public Dictionary<string, ClipSource> Clips;
        }

        private readonly struct ClipSource
        {
            public ClipSource(string path, float frameRate, bool loop, bool isExistingClip)
            {
                Path = path;
                FrameRate = frameRate;
                Loop = loop;
                IsExistingClip = isExistingClip;
            }

            public string Path { get; }
            public float FrameRate { get; }
            public bool Loop { get; }
            public bool IsExistingClip { get; }
        }

        private enum AudioKind
        {
            Bat,
            Mimic,
            Rat,
            Slime,
            Wolf
        }
    }
}
