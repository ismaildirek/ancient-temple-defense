using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AncientTempleDefense.Allies;
using AncientTempleDefense.Economy;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class DefenseLootSetup
    {
        private const string GeneratedRoot = "Assets/AncientTempleDefense/Generated";
        private const string PrefabRoot = GeneratedRoot + "/Prefabs";
        private const string AllyAnimationRoot = GeneratedRoot + "/Animations/Allies";
        private const string AnimatorRoot = GeneratedRoot + "/Animators";
        private const string MapScenePath = "Assets/Scenes/Map.unity";
        private const string MartialSpriteRoot = "Assets/Characters_assets/Martial Hero/Sprites";
        private const string HeroAnimationRoot = "Assets/Characters_assets/Hero Knight - Pixel Art/Animations";
        private const string HeroSpritePath = "Assets/Characters_assets/Hero Knight - Pixel Art/Sprites/HeroKnight.png";
        private const string CoinTexturePath = "Assets/2D Pixel Item Pack/Heavy Outline/S_ItemHeavyOutline_CoinGold_00.png";
        private const string PotionTexturePath = "Assets/2D Pixel Item Pack/Heavy Outline/S_ItemHeavyOutline_PotionRed_00.png";
        private const string PixelFontPath = "Assets/Thaleah_PixelFont/Materials/ThaleahFat_TTF.ttf";

        [MenuItem("Tools/Ancient Temple Defense/Build Loot And Defense Shop")]
        public static void Configure()
        {
            EnsureFolders();
            LateGameEnemyGeometrySetup.Configure();
            NormalizeFolderBottoms(MartialSpriteRoot, 0.5f);
            NormalizeTextureBottoms(HeroSpritePath, 0.5f);
            ConfigureItemTexture(CoinTexturePath);
            ConfigureItemTexture(PotionTexturePath);

            WorldPickup coin = CreatePickupPrefab(
                "CoinPickup", CoinTexturePath, PickupType.Coin, 1, Vector3.one * 1.55f);
            WorldPickup potion = CreatePickupPrefab(
                "HealthPotionPickup", PotionTexturePath, PickupType.HealthPotion, 50, Vector3.one * 1.45f);
            FriendlyDefender martial = CreateMartialHero();
            FriendlyDefender knight = CreateHeroKnight();
            ConfigurePlayerWallet();
            ConfigureEnemyPrefabs(coin, potion);
            ConfigureScene(martial, knight);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Loot, coin, dost savascilar ve 5-wave savunma magazasi kuruldu.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/AncientTempleDefense", "Generated");
            EnsureFolder(GeneratedRoot, "Animations");
            EnsureFolder(GeneratedRoot + "/Animations", "Allies");
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

        private static void ConfigureItemTexture(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException("Item texture bulunamadi: " + path);
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static WorldPickup CreatePickupPrefab(string name, string texturePath, PickupType type, int value, Vector3 scale)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath)
                ?? throw new InvalidOperationException("Item sprite bulunamadi: " + texturePath);
            GameObject root = new(name);
            root.transform.localScale = scale;
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 16;
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = Mathf.Max(0.12f, Mathf.Min(sprite.bounds.extents.x, sprite.bounds.extents.y) * 0.8f);
            WorldPickup pickup = root.AddComponent<WorldPickup>();
            SerializedObject serialized = new(pickup);
            serialized.FindProperty("esyaTuru").enumValueIndex = (int)type;
            serialized.FindProperty("deger").intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabRoot + "/" + name + ".prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return prefab.GetComponent<WorldPickup>();
        }

        private static FriendlyDefender CreateMartialHero()
        {
            string folder = AllyAnimationRoot + "/MartialHero";
            EnsureFolder(AllyAnimationRoot, "MartialHero");
            Dictionary<string, AnimationClip> clips = new()
            {
                ["Idle"] = CreateTextureClip(MartialSpriteRoot + "/Idle.png", folder + "/Idle.anim", "Idle", true, 12f),
                ["Run"] = CreateTextureClip(MartialSpriteRoot + "/Run.png", folder + "/Run.anim", "Run", true, 14f),
                ["Attack1"] = CreateTextureClip(MartialSpriteRoot + "/Attack1.png", folder + "/Attack1.anim", "Attack1", false, 14f),
                ["Attack2"] = CreateTextureClip(MartialSpriteRoot + "/Attack2.png", folder + "/Attack2.anim", "Attack2", false, 14f),
                ["Hit"] = CreateTextureClip(MartialSpriteRoot + "/Take Hit.png", folder + "/Hit.anim", "Hit", false, 12f),
                ["Death"] = CreateTextureClip(MartialSpriteRoot + "/Death.png", folder + "/Death.anim", "Death", false, 12f)
            };
            AnimatorController controller = CreateController(AnimatorRoot + "/MartialHeroAlly.controller", clips, "Idle");
            return CreateAllyPrefab("MartialHeroAlly", controller, FirstSprite(clips["Idle"]), Vector3.one * 3.1f, 180, 1, 2.45f);
        }

        private static FriendlyDefender CreateHeroKnight()
        {
            string folder = AllyAnimationRoot + "/HeroKnight";
            EnsureFolder(AllyAnimationRoot, "HeroKnight");
            Dictionary<string, AnimationClip> clips = new()
            {
                ["Idle"] = CopyClip(HeroAnimationRoot + "/HeroKnight_Idle.anim", folder + "/Idle.anim", "Idle", true),
                ["Run"] = CopyClip(HeroAnimationRoot + "/HeroKnight_Run.anim", folder + "/Run.anim", "Run", true),
                ["Attack1"] = CopyClip(HeroAnimationRoot + "/HeroKnight_Attack1.anim", folder + "/Attack1.anim", "Attack1", false),
                ["Attack2"] = CopyClip(HeroAnimationRoot + "/HeroKnight_Attack2.anim", folder + "/Attack2.anim", "Attack2", false),
                ["Hit"] = CopyClip(HeroAnimationRoot + "/HeroKnight_Hurt.anim", folder + "/Hit.anim", "Hit", false),
                ["Death"] = CopyClip(HeroAnimationRoot + "/HeroKnight_DeathNoBlood.anim", folder + "/Death.anim", "Death", false)
            };
            AnimatorController controller = CreateController(AnimatorRoot + "/HeroKnightAlly.controller", clips, "Idle");
            return CreateAllyPrefab("HeroKnightAlly", controller, FirstSprite(clips["Idle"]), Vector3.one * 1.45f, 240, 2, 2.15f);
        }

        private static AnimationClip CreateTextureClip(string texturePath, string outputPath, string stateName, bool loop, float frameRate)
        {
            List<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>().OrderBy(sprite => SpriteIndex(sprite.name)).ToList();
            if (sprites.Count == 0)
            {
                throw new InvalidOperationException("Animasyon sprite kareleri bulunamadi: " + texturePath);
            }
            DeleteAssetIfExists(outputPath);
            AnimationClip clip = new() { name = stateName, frameRate = frameRate };
            ObjectReferenceKeyframe[] frames = new ObjectReferenceKeyframe[sprites.Count + 1];
            for (int index = 0; index < sprites.Count; index++)
            {
                frames[index] = new ObjectReferenceKeyframe { time = index / frameRate, value = sprites[index] };
            }
            frames[^1] = new ObjectReferenceKeyframe
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
            AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, outputPath);
            return clip;
        }

        private static AnimationClip CopyClip(string sourcePath, string outputPath, string stateName, bool loop)
        {
            AnimationClip source = AssetDatabase.LoadAssetAtPath<AnimationClip>(sourcePath)
                ?? throw new InvalidOperationException("Hero Knight klibi bulunamadi: " + sourcePath);
            DeleteAssetIfExists(outputPath);
            AnimationClip copy = new();
            EditorUtility.CopySerialized(source, copy);
            copy.name = stateName;
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(copy);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(copy, settings);
            AssetDatabase.CreateAsset(copy, outputPath);
            return copy;
        }

        private static AnimatorController CreateController(string outputPath, IReadOnlyDictionary<string, AnimationClip> clips, string defaultState)
        {
            DeleteAssetIfExists(outputPath);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(outputPath);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState defaultAnimatorState = null;
            int index = 0;
            foreach (KeyValuePair<string, AnimationClip> pair in clips)
            {
                AnimatorState state = machine.AddState(pair.Key, new Vector3((index % 3) * 240f, (index / 3) * 90f, 0f));
                state.motion = pair.Value;
                state.writeDefaultValues = true;
                if (pair.Key == defaultState)
                {
                    defaultAnimatorState = state;
                }
                index++;
            }
            machine.defaultState = defaultAnimatorState;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static FriendlyDefender CreateAllyPrefab(string name, RuntimeAnimatorController controller, Sprite idleSprite, Vector3 scale, int health, int damage, float speed)
        {
            GameObject root = new(name);
            root.transform.localScale = scale;
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = idleSprite;
            renderer.sortingOrder = 9;
            Animator animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            CapsuleCollider2D collider = root.AddComponent<CapsuleCollider2D>();
            ConfigureTightCollider(idleSprite, collider);
            collider.isTrigger = true;
            FriendlyDefender defender = root.AddComponent<FriendlyDefender>();
            SerializedObject serialized = new(defender);
            serialized.FindProperty("azamiCan").intValue = health;
            serialized.FindProperty("saldiriHasari").intValue = damage;
            serialized.FindProperty("hareketHizi").floatValue = speed;
            serialized.FindProperty("kaynakVarsayilanYonuSola").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabRoot + "/" + name + ".prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return prefab.GetComponent<FriendlyDefender>();
        }

        private static void ConfigurePlayerWallet()
        {
            string path = PrefabRoot + "/BlackKnightPlayer.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.GetComponent<PlayerWallet>() == null)
                {
                    root.AddComponent<PlayerWallet>();
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureEnemyPrefabs(WorldPickup coin, WorldPickup potion)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot });
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    EnemyCombatant enemy = root.GetComponent<EnemyCombatant>();
                    if (enemy == null)
                    {
                        continue;
                    }
                    Collider2D collider = root.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        collider.isTrigger = true;
                    }
                    Animator animator = root.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                    }
                    BossEnemyBrain boss = root.GetComponent<BossEnemyBrain>();
                    if (boss != null)
                    {
                        SerializedObject bossSerialized = new(boss);
                        bossSerialized.FindProperty("kaynakVarsayilanYonuSola").boolValue = root.name != "Boss2Enemy";
                        bossSerialized.ApplyModifiedPropertiesWithoutUndo();
                    }
                    EnemyLootDropper dropper = root.GetComponent<EnemyLootDropper>();
                    if (dropper == null)
                    {
                        dropper = root.AddComponent<EnemyLootDropper>();
                    }
                    SerializedObject serialized = new(dropper);
                    serialized.FindProperty("coinDusmeIhtimali").floatValue = 0.40f;
                    serialized.FindProperty("iksirDusmeIhtimali").floatValue = 0.20f;
                    serialized.FindProperty("coinPrefabi").objectReferenceValue = coin;
                    serialized.FindProperty("canIksiriPrefabi").objectReferenceValue = potion;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void ConfigureScene(FriendlyDefender martial, FriendlyDefender knight)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MapScenePath, OpenSceneMode.Single);
            GameObject martialPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + "/MartialHeroAlly.prefab")
                ?? throw new InvalidOperationException("Martial Hero dost prefabi yeniden yuklenemedi.");
            GameObject knightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + "/HeroKnightAlly.prefab")
                ?? throw new InvalidOperationException("Hero Knight dost prefabi yeniden yuklenemedi.");
            BlackKnightPlayerController player = UnityEngine.Object.FindFirstObjectByType<BlackKnightPlayerController>();
            EnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EnemyWaveSpawner>();
            if (player == null || spawner == null)
            {
                throw new InvalidOperationException("Map sahnesinde oyuncu veya wave sistemi bulunamadi.");
            }
            PlayerWallet wallet = player.GetComponent<PlayerWallet>();
            if (wallet == null)
            {
                wallet = player.gameObject.AddComponent<PlayerWallet>();
            }
            GameObject gameplay = GameObject.Find("Gameplay");
            Transform existing = gameplay != null ? gameplay.transform.Find("DefenseShopSystem") : null;
            GameObject shopObject = existing != null ? existing.gameObject : new GameObject("DefenseShopSystem");
            if (gameplay != null)
            {
                shopObject.transform.SetParent(gameplay.transform);
            }
            DefenseShopPanel shop = shopObject.GetComponent<DefenseShopPanel>();
            if (shop == null)
            {
                shop = shopObject.AddComponent<DefenseShopPanel>();
            }
            Font font = AssetDatabase.LoadAssetAtPath<Font>(PixelFontPath)
                ?? throw new InvalidOperationException("Thaleah Pixel Font bulunamadi.");
            SerializedObject shopSerialized = new(shop);
            shopSerialized.FindProperty("pikselYaziTipi").objectReferenceValue = font;
            shopSerialized.FindProperty("oyuncu").objectReferenceValue = player;
            shopSerialized.FindProperty("coinCuzdani").objectReferenceValue = wallet;
            shopSerialized.FindProperty("martialHeroPrefabi").objectReferenceValue = martialPrefab;
            shopSerialized.FindProperty("heroKnightPrefabi").objectReferenceValue = knightPrefab;
            shopSerialized.FindProperty("askerFiyati").intValue = 20;
            shopSerialized.FindProperty("okcuFiyati").intValue = 50;
            shopSerialized.FindProperty("buyucuFiyati").intValue = 60;
            shopSerialized.FindProperty("enFazlaDost").intValue = 3;
            shopSerialized.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject spawnerSerialized = new(spawner);
            spawnerSerialized.FindProperty("savunmaMagazasiPaneli").objectReferenceValue = shop;
            spawnerSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, MapScenePath))
            {
                throw new InvalidOperationException("Map sahnesi kaydedilemedi.");
            }
        }

        private static void NormalizeFolderBottoms(string folder, float pivotX)
        {
            string[] textures = AssetDatabase.FindAssets("t:Texture2D", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => string.Equals(Path.GetDirectoryName(path)?.Replace('\\', '/'), folder, StringComparison.Ordinal))
                .ToArray();
            foreach (string texture in textures)
            {
                NormalizeTextureBottoms(texture, pivotX);
            }
        }

        private static void NormalizeTextureBottoms(string path, float pivotX)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Sprite)
            {
                return;
            }
            Dictionary<string, Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>().ToDictionary(sprite => sprite.name, StringComparer.Ordinal);
            if (sprites.Count == 0)
            {
                return;
            }
            SpriteDataProviderFactories factories = new();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            SpriteRect[] rects = provider.GetSpriteRects();
            bool changed = false;
            foreach (SpriteRect rect in rects)
            {
                if (!sprites.TryGetValue(rect.name, out Sprite sprite)
                    || sprite.vertices.Length == 0
                    || rect.rect.height <= 0f)
                {
                    continue;
                }
                Vector2 currentPivot = new(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
                float visibleBottom = sprite.vertices.Min(vertex => vertex.y);
                Vector2 aligned = new(
                    pivotX,
                    Mathf.Clamp01(currentPivot.y + visibleBottom * sprite.pixelsPerUnit / rect.rect.height));
                if (rect.alignment == SpriteAlignment.Custom
                    && Vector2.SqrMagnitude(rect.pivot - aligned) < 0.000001f)
                {
                    continue;
                }
                rect.alignment = SpriteAlignment.Custom;
                rect.pivot = aligned;
                changed = true;
            }
            if (!changed)
            {
                return;
            }
            provider.SetSpriteRects(rects);
            provider.Apply();
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void ConfigureTightCollider(Sprite sprite, CapsuleCollider2D collider)
        {
            Bounds visible = new(sprite.vertices[0], Vector3.zero);
            for (int index = 1; index < sprite.vertices.Length; index++)
            {
                visible.Encapsulate(sprite.vertices[index]);
            }
            float width = Mathf.Max(0.16f, visible.size.x * 0.46f);
            float height = Mathf.Max(0.25f, visible.size.y * 0.82f);
            collider.size = new Vector2(width, height);
            collider.offset = new Vector2(visible.center.x, visible.min.y + height * 0.5f);
            collider.direction = CapsuleDirection2D.Vertical;
        }

        private static Sprite FirstSprite(AnimationClip clip)
        {
            EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(clip)
                .First(item => item.propertyName == "m_Sprite");
            return AnimationUtility.GetObjectReferenceCurve(clip, binding)[0].value as Sprite;
        }

        private static int SpriteIndex(string name)
        {
            int underscore = name.LastIndexOf('_');
            return underscore >= 0 && int.TryParse(name[(underscore + 1)..], out int value) ? value : 0;
        }

        private static void DeleteAssetIfExists(string path)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }
    }
}
