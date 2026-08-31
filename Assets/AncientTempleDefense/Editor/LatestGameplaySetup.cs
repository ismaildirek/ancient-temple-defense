using System;
using AncientTempleDefense.Economy;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AncientTempleDefense.Editor
{
    public static class LatestGameplaySetup
    {
        private const string PrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";
        private const string TemplePotionSprite = "Assets/2D Pixel Item Pack/Heavy Outline/S_ItemHeavyOutline_PotionBlue_02.png";
        private const string FontPath = "Assets/Thaleah_PixelFont/Materials/ThaleahFat_TTF.ttf";

        [MenuItem("Tools/Ancient Temple Defense/Apply Latest Gameplay Fixes")]
        public static void Apply()
        {
            WorldPickup templePotion = CreateTemplePotion();
            NormalizeEnemiesAndLoot(templePotion);
            ConfigureMapGameOver();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("PLAY hareketi, Wolf yönü, düşman ölçekleri, tapınak iksiri ve Game Over ekranı uygulandı.");
        }

        private static WorldPickup CreateTemplePotion()
        {
            TextureImporter importer = AssetImporter.GetAtPath(TemplePotionSprite) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TemplePotionSprite) ?? throw new InvalidOperationException("Tapınak iksiri sprite bulunamadı.");
            GameObject root = new("TemplePotionPickup"); root.transform.localScale = Vector3.one * 1.45f;
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = 16;
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>(); collider.isTrigger = true; collider.radius = Mathf.Max(.12f, Mathf.Min(sprite.bounds.extents.x, sprite.bounds.extents.y) * .8f);
            WorldPickup pickup = root.AddComponent<WorldPickup>();
            SerializedObject serialized = new(pickup); serialized.FindProperty("esyaTuru").enumValueIndex = (int)PickupType.TemplePotion; serialized.FindProperty("deger").intValue = 50; serialized.ApplyModifiedPropertiesWithoutUndo();
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabRoot + "/TemplePotionPickup.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return saved.GetComponent<WorldPickup>();
        }

        private static void NormalizeEnemiesAndLoot(WorldPickup templePotion)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    EnemyCombatant enemy = root.GetComponent<EnemyCombatant>();
                    if (enemy == null) continue;
                    float intendedScale = IntendedScale(root.name);
                    if (intendedScale > 0f)
                    {
                        root.transform.localScale = Vector3.one * intendedScale;
                        SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
                        ConfigureCollider(root.GetComponent<CapsuleCollider2D>(), renderer != null ? renderer.sprite : null, intendedScale);
                    }
                    EnemyBrain brain = root.GetComponent<EnemyBrain>();
                    if (brain != null && root.name == "DarkWolfEnemy")
                    {
                        SerializedObject brainData = new(brain);
                        brainData.FindProperty("kaynakVarsayilanYonuSola").boolValue = true;
                        brainData.ApplyModifiedPropertiesWithoutUndo();
                    }
                    EnemyLootDropper loot = root.GetComponent<EnemyLootDropper>();
                    if (loot != null)
                    {
                        SerializedObject lootData = new(loot);
                        lootData.FindProperty("tapinakIksiriDusmeIhtimali").floatValue = .30f;
                        lootData.FindProperty("tapinakIksiriPrefabi").objectReferenceValue = templePotion;
                        lootData.ApplyModifiedPropertiesWithoutUndo();
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }

        private static float IntendedScale(string prefabName)
        {
            return prefabName switch
            {
                "SkeletonEnemy" or "GoblinEnemy" or "MushroomEnemy" or "FlyingEyeEnemy" => 2.5f,
                "NewEnemy1" or "NewEnemy2" or "NewEnemy4" => 2.5f,
                "NewEnemy3" => 2.65f,
                "NewEnemy5" => 2.55f,
                "MimicEnemy" => 1.4f,
                "RatRaiderEnemy" => 2.6f,
                "ExplodingSlimeEnemy" => 1.35f,
                "DarkWolfEnemy" => 1.4f,
                "BatFlyingEnemy" => 2f,
                _ => 0f
            };
        }

        private static void ConfigureCollider(CapsuleCollider2D collider, Sprite sprite, float scale)
        {
            if (collider == null || sprite == null || sprite.vertices == null || sprite.vertices.Length == 0) return;
            Bounds bounds = new(sprite.vertices[0], Vector3.zero);
            for (int index = 1; index < sprite.vertices.Length; index++) bounds.Encapsulate(sprite.vertices[index]);
            float min = .46f / Mathf.Max(.01f, Mathf.Abs(scale));
            Vector2 size = new(Mathf.Max(min, bounds.size.x * .78f), Mathf.Max(min, bounds.size.y * .94f));
            collider.size = size;
            collider.offset = new Vector2(bounds.center.x, bounds.min.y + size.y * .5f);
            collider.direction = size.x > size.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
            collider.isTrigger = true;
        }
        private static void ConfigureMapGameOver()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Map.unity", OpenSceneMode.Single);
            GameObject systems = GameObject.Find("GameOverSystem") ?? new GameObject("GameOverSystem");
            GameOverPanel panel = systems.GetComponent<GameOverPanel>() ?? systems.AddComponent<GameOverPanel>();
            SerializedObject data = new(panel); data.FindProperty("pixelYazıTipi").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(FontPath); data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
    }
}
