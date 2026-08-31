using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.U2D;

namespace AncientTempleDefense.Editor
{
    public static class LateGameEnemyGeometrySetup
    {
        private const string PrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";
        private const string AtlasPath = "Assets/AncientTempleDefense/Generated/LateGameEnemies.spriteatlas";
        private const string NewEnemyRoot = "Assets/Characters_assets/new_enemy/Sprites";
        private const string BossOneSpriteRoot = "Assets/Characters_assets/BOSS_1/Sprite Sheet";
        private const string BossTwoSpriteRoot = "Assets/Characters_assets/BOSS_2/Sprites";

        private static readonly PivotDefinition[] PivotDefinitions =
        {
            new(NewEnemyRoot + "/Enemy1", new Vector2(0.220f, 0f)),
            new(NewEnemyRoot + "/Enemy2", new Vector2(0.371f, 0f)),
            new(NewEnemyRoot + "/Enemy3", new Vector2(0.477f, 0f)),
            new(NewEnemyRoot + "/Enemy4", new Vector2(0.453f, 0f)),
            new(NewEnemyRoot + "/Enemy5", new Vector2(0.459f, 0f)),
            new(BossOneSpriteRoot, new Vector2(0.752f, 0.011f)),
            new(BossTwoSpriteRoot, new Vector2(0.544f, 0.332f))
        };

        private static readonly string[] PrefabNames =
        {
            "NewEnemy1",
            "NewEnemy2",
            "NewEnemy3",
            "NewEnemy4",
            "NewEnemy5",
            "Boss1Enemy",
            "Boss2Enemy"
        };

        [MenuItem("Tools/Ancient Temple Defense/Repair Late Game Enemy Geometry")]
        public static void Configure()
        {
            ConfigureSpritePivots();
            ConfigureGeneratedPrefabs();
            ConfigureSpriteAtlas();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Yeni düşman ve boss pivot, collider ve çizim optimizasyonu tamamlandı.");
        }

        internal static void ConfigureSpritePivots()
        {
            foreach (PivotDefinition definition in PivotDefinitions)
            {
                string[] texturePaths = AssetDatabase.FindAssets("t:Texture2D", new[] { definition.Folder })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => string.Equals(
                        Path.GetDirectoryName(path)?.Replace('\\', '/'),
                        definition.Folder,
                        StringComparison.Ordinal))
                    .ToArray();

                foreach (string texturePath in texturePaths)
                {
                    ConfigureTexturePivot(texturePath, definition.Pivot);
                }
            }
        }

        internal static void ConfigureGeneratedPrefabs()
        {
            foreach (string prefabName in PrefabNames)
            {
                string path = $"{PrefabRoot}/{prefabName}.prefab";
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    SpriteRenderer renderer = root.GetComponent<SpriteRenderer>()
                        ?? throw new InvalidOperationException($"SpriteRenderer bulunamadı: {prefabName}");
                    CapsuleCollider2D collider = root.GetComponent<CapsuleCollider2D>()
                        ?? throw new InvalidOperationException($"CapsuleCollider2D bulunamadı: {prefabName}");
                    Animator animator = root.GetComponent<Animator>()
                        ?? throw new InvalidOperationException($"Animator bulunamadı: {prefabName}");

                    ConfigureCollider(renderer.sprite, collider);
                    if (string.Equals(prefabName, "NewEnemy3", StringComparison.Ordinal))
                    {
                        ExtendColliderUpward(collider, 0.28f);
                    }

                    animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        internal static void ConfigureSpriteAtlas()
        {
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas { name = "LateGameEnemies" };
                AssetDatabase.CreateAsset(atlas, AtlasPath);
            }

            UnityEngine.Object[] existingPackables = atlas.GetPackables();
            if (existingPackables.Length > 0)
            {
                atlas.Remove(existingPackables);
            }

            UnityEngine.Object[] enemyFolders = Enumerable.Range(1, 5)
                .Select(index => AssetDatabase.LoadAssetAtPath<DefaultAsset>($"{NewEnemyRoot}/Enemy{index}"))
                .Where(folder => folder != null)
                .Cast<UnityEngine.Object>()
                .ToArray();
            atlas.Add(enemyFolders);

            SpriteAtlasPackingSettings packing = atlas.GetPackingSettings();
            packing.enableRotation = false;
            packing.enableTightPacking = true;
            packing.padding = 2;
            atlas.SetPackingSettings(packing);

            SpriteAtlasTextureSettings texture = atlas.GetTextureSettings();
            texture.filterMode = FilterMode.Point;
            texture.generateMipMaps = false;
            texture.readable = false;
            texture.sRGB = true;
            atlas.SetTextureSettings(texture);

            TextureImporterPlatformSettings platform = atlas.GetPlatformSettings("DefaultTexturePlatform");
            platform.name = "DefaultTexturePlatform";
            platform.overridden = false;
            platform.maxTextureSize = 2048;
            platform.textureCompression = TextureImporterCompression.Uncompressed;
            atlas.SetPlatformSettings(platform);
            atlas.SetIncludeInBuild(true);
            EditorUtility.SetDirty(atlas);
            SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget);
        }

        private static void ConfigureTexturePivot(string texturePath, Vector2 pivot)
        {
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Sprite)
            {
                return;
            }

            SpriteDataProviderFactories factories = new();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            SpriteRect[] spriteRects = provider.GetSpriteRects();
            bool spriteRectsChanged = false;
            foreach (SpriteRect spriteRect in spriteRects)
            {
                Vector2 desiredPivot = new(
                    pivot.x,
                    spriteRect.alignment == SpriteAlignment.Custom ? spriteRect.pivot.y : pivot.y);
                if (spriteRect.alignment == SpriteAlignment.Custom
                    && Approximately(spriteRect.pivot, desiredPivot))
                {
                    continue;
                }

                spriteRect.alignment = SpriteAlignment.Custom;
                spriteRect.pivot = desiredPivot;
                spriteRectsChanged = true;
            }

            bool textureSettingsChanged = texturePath.StartsWith(NewEnemyRoot, StringComparison.Ordinal)
                && importer.textureCompression != TextureImporterCompression.Uncompressed;
            if (!spriteRectsChanged && !textureSettingsChanged)
            {
                AlignVisibleBottoms(texturePath, pivot.x);
                return;
            }

            if (spriteRectsChanged)
            {
                provider.SetSpriteRects(spriteRects);
                provider.Apply();
            }

            if (textureSettingsChanged)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
            }

            importer.SaveAndReimport();
            AlignVisibleBottoms(texturePath, pivot.x);
        }

        private static void AlignVisibleBottoms(string texturePath, float pivotX)
        {
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            Dictionary<string, Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .ToDictionary(sprite => sprite.name, StringComparer.Ordinal);
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
                    || sprite.vertices == null
                    || sprite.vertices.Length == 0
                    || rect.rect.height <= 0f)
                {
                    continue;
                }

                float visibleBottom = sprite.vertices.Min(vertex => vertex.y);
                float normalizedOffset = visibleBottom * sprite.pixelsPerUnit / rect.rect.height;
                Vector2 alignedPivot = new(
                    pivotX,
                    Mathf.Clamp01(rect.pivot.y + normalizedOffset));
                if (rect.alignment == SpriteAlignment.Custom
                    && Approximately(rect.pivot, alignedPivot))
                {
                    continue;
                }

                rect.alignment = SpriteAlignment.Custom;
                rect.pivot = alignedPivot;
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            provider.SetSpriteRects(rects);
            provider.Apply();
            importer.SaveAndReimport();
        }

        private static void ConfigureCollider(Sprite sprite, CapsuleCollider2D collider)
        {
            if (sprite == null || sprite.vertices.Length == 0)
            {
                throw new InvalidOperationException("Collider için görünür sprite sınırı bulunamadı.");
            }

            Bounds visible = new(sprite.vertices[0], Vector3.zero);
            for (int index = 1; index < sprite.vertices.Length; index++)
            {
                visible.Encapsulate(sprite.vertices[index]);
            }

            float width = Mathf.Max(0.10f, visible.size.x * 0.82f);
            float height = Mathf.Max(0.10f, visible.size.y * 0.94f);
            float bottom = visible.min.y + visible.size.y * 0.02f;
            collider.size = new Vector2(width, height);
            collider.offset = new Vector2(visible.center.x, bottom + height * 0.5f);
            collider.direction = width > height
                ? CapsuleDirection2D.Horizontal
                : CapsuleDirection2D.Vertical;
            collider.isTrigger = true;
        }

        private static void ExtendColliderUpward(CapsuleCollider2D collider, float minimumHeight)
        {
            if (collider.size.y >= minimumHeight)
            {
                return;
            }

            float bottom = collider.offset.y - collider.size.y * 0.5f;
            Vector2 size = collider.size;
            size.y = minimumHeight;
            collider.size = size;
            collider.offset = new Vector2(collider.offset.x, bottom + size.y * 0.5f);
            collider.direction = size.x > size.y
                ? CapsuleDirection2D.Horizontal
                : CapsuleDirection2D.Vertical;
        }

        private static bool Approximately(Vector2 left, Vector2 right)
        {
            return Mathf.Abs(left.x - right.x) < 0.0001f
                && Mathf.Abs(left.y - right.y) < 0.0001f;
        }

        private readonly struct PivotDefinition
        {
            public PivotDefinition(string folder, Vector2 pivot)
            {
                Folder = folder;
                Pivot = pivot;
            }

            public string Folder { get; }
            public Vector2 Pivot { get; }
        }
    }
}
