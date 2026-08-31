using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AncientTempleDefense.Scene;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class ProjectHealthOptimizer
    {
        private const string PrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";
        private const string ReportPath = "Assets/AncientTempleDefense/Generated/Reports/ProjectHealthReport.txt";
        private static readonly string[] ScenePaths = { "Assets/Scenes/giris.unity", "Assets/Scenes/Map.unity" };

        [MenuItem("Tools/Ancient Temple Defense/Run Project Health Optimizer")]
        public static void Apply()
        {
            EnsureReportFolder();
            int arenaFixes = FixEnvironmentSorting();
            int animatorFixes = OptimizePrefabPresentation();
            int spriteImportFixes = OptimizeReferencedSpriteImports();
            int audioFixes = OptimizeNewAudioImports();
            int missingScripts = CountMissingScripts();

            List<string> report = new()
            {
                "Ancient Temple Defense - Project Health Report",
                $"UTC: {DateTime.UtcNow:O}",
                $"Arena sorting fixes: {arenaFixes}",
                $"Animator/collider prefab fixes: {animatorFixes}",
                $"Referenced sprite import fixes: {spriteImportFixes}",
                $"Audio import fixes: {audioFixes}",
                $"Missing scripts: {missingScripts}",
                "Runtime optimization: pooled combat VFX, cached temple lookup, no redundant target fallback search",
                "Expected runtime limits: max 8 enemies, max 48 particles per system, max 10 idle pooled effects per prefab"
            };
            File.WriteAllLines(ReportPath, report);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (missingScripts > 0)
                throw new InvalidOperationException($"Sağlık kontrolünde {missingScripts} eksik script bulundu. Rapor: {ReportPath}");
            Debug.Log($"Proje sağlık optimizasyonu tamamlandı. Sprite={spriteImportFixes}, Animator/Collider={animatorFixes}, MissingScript=0. Rapor: {ReportPath}");
        }

        private static int FixEnvironmentSorting()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Map.unity", OpenSceneMode.Single);
            GameplaySceneMarker marker = UnityEngine.Object.FindFirstObjectByType<GameplaySceneMarker>();
            Transform presentation = marker != null ? marker.transform.Find("EnvironmentPresentation") : null;
            if (presentation == null) return 0;

            int changes = 0;
            changes += SetSortingOrder(presentation, "ArenaForeground", 6);
            changes += SetSortingOrder(presentation, "TempleWardSeal", 3);
            changes += SetSortingOrder(presentation, "EnemyPortalLeft", 4);
            changes += SetSortingOrder(presentation, "EnemyPortalRight", 4);
            if (changes <= 0) return 0;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return changes;
        }

        private static int SetSortingOrder(Transform parent, string childName, int sortingOrder)
        {
            Transform child = parent.Find(childName);
            SpriteRenderer renderer = child != null ? child.GetComponent<SpriteRenderer>() : null;
            if (renderer == null || renderer.sortingOrder == sortingOrder) return 0;
            renderer.sortingOrder = sortingOrder;
            return 1;
        }

        private static int OptimizePrefabPresentation()
        {
            int changes = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                bool changed = false;
                try
                {
                    foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
                    {
                        if (animator.cullingMode == AnimatorCullingMode.CullUpdateTransforms) continue;
                        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                        changed = true;
                    }

                    if (root.GetComponent<AncientTempleDefense.Enemies.EnemyCombatant>() != null)
                    {
                        Collider2D collider = root.GetComponent<Collider2D>();
                        if (collider != null && !collider.isTrigger)
                        {
                            collider.isTrigger = true;
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        changes++;
                    }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            return changes;
        }

        private static int OptimizeReferencedSpriteImports()
        {
            HashSet<string> texturePaths = new(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot }))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (SpriteRenderer renderer in prefab.GetComponentsInChildren<SpriteRenderer>(true))
                    AddSpritePath(renderer.sprite, texturePaths);
                foreach (Animator animator in prefab.GetComponentsInChildren<Animator>(true))
                {
                    if (animator.runtimeAnimatorController == null) continue;
                    foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                    foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    foreach (ObjectReferenceKeyframe frame in AnimationUtility.GetObjectReferenceCurve(clip, binding))
                        if (frame.value is Sprite sprite) AddSpritePath(sprite, texturePaths);
                }
            }

            int changes = 0;
            foreach (string path in texturePaths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.textureType != TextureImporterType.Sprite) continue;
                bool changed = importer.mipmapEnabled || importer.filterMode != FilterMode.Point || importer.wrapMode != TextureWrapMode.Clamp;
                if (!changed) continue;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
                changes++;
            }
            return changes;
        }

        private static int OptimizeNewAudioImports()
        {
            string[] paths =
            {
                "Assets/Musics/SwordSoundPack/SWORD_12.wav",
                "Assets/Musics/Leohpaz/RPG_Essentials_Free/8_Atk_Magic_SFX/18_Thunder_02.wav",
                "Assets/Musics/Leohpaz/RPG_Essentials_Free/8_Atk_Magic_SFX/30_Earth_02.wav"
            };
            int changes = 0;
            foreach (string path in paths)
            {
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                bool changed = settings.loadType != AudioClipLoadType.DecompressOnLoad
                    || settings.compressionFormat != AudioCompressionFormat.PCM
                    || !settings.preloadAudioData || importer.loadInBackground;
                if (!changed) continue;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                settings.quality = 1f;
                importer.defaultSampleSettings = settings;
                settings.preloadAudioData = true;
                importer.loadInBackground = false;
                importer.SaveAndReimport();
                changes++;
            }
            return changes;
        }

        private static int CountMissingScripts()
        {
            int missing = 0;
            foreach (string scenePath in ScenePaths)
            {
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (GameObject root in scene.GetRootGameObjects()) missing += CountMissingRecursive(root);
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot }))
            {
                GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GUIDToAssetPath(guid));
                try { missing += CountMissingRecursive(root); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            return missing;
        }

        private static int CountMissingRecursive(GameObject root)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root);
            foreach (Transform child in root.transform) count += CountMissingRecursive(child.gameObject);
            return count;
        }

        private static void AddSpritePath(Sprite sprite, ISet<string> paths)
        {
            if (sprite == null) return;
            string path = AssetDatabase.GetAssetPath(sprite);
            if (!string.IsNullOrEmpty(path)) paths.Add(path);
        }

        private static void EnsureReportFolder()
        {
            const string folder = "Assets/AncientTempleDefense/Generated/Reports";
            if (AssetDatabase.IsValidFolder(folder)) return;
            if (!AssetDatabase.IsValidFolder("Assets/AncientTempleDefense/Generated"))
                AssetDatabase.CreateFolder("Assets/AncientTempleDefense", "Generated");
            AssetDatabase.CreateFolder("Assets/AncientTempleDefense/Generated", "Reports");
        }
    }
}
