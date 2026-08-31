using System;
using System.Collections.Generic;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Vfx;
using UnityEditor;
using UnityEngine;

namespace AncientTempleDefense.Editor
{
    public static class VfxAnimationScaleFix
    {
        private const string PrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";
        private const string BloodEffectPath = "Assets/100BestEffectPack/Effects/BloodEffect/BloodEffect2.prefab";
        private const string DarkEffectPath = "Assets/100BestEffectPack/Effects/DarkEffect/DarkEffect5.prefab";
        private const string PoisonHitPath = "Assets/100BestEffectPack/Effects/PoisonEffect/PoisonEffect2.prefab";
        private const string PoisonDeathPath = "Assets/100BestEffectPack/Effects/PoisonEffect/PoisonEffect5.prefab";
        private const string ExplosionPath = "Assets/100BestEffectPack/Effects/ExplosionEffect/ExplosionEffect4.prefab";

        private static readonly IReadOnlyDictionary<string, float> SpecialScales =
            new Dictionary<string, float>(StringComparer.Ordinal)
            {
                ["BatFlyingEnemy"] = 0.028f,
                ["MimicEnemy"] = 0.032f,
                ["RatRaiderEnemy"] = 0.024f,
                ["ExplodingSlimeEnemy"] = 0.026f,
                ["DarkWolfEnemy"] = 0.032f
            };

        [MenuItem("Tools/Ancient Temple Defense/Efekt Animasyonu Boyutlarını Düzelt")]
        public static void Apply()
        {
            int updated = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    EnemyVfxController vfx = root.GetComponent<EnemyVfxController>();
                    if (vfx == null)
                    {
                        continue;
                    }

                    bool slime = root.GetComponent<ExplodingEnemy>() != null;
                    bool bossOrUndead = root.GetComponent<BossEnemyBrain>() != null
                        || root.name.Contains("Boss", StringComparison.OrdinalIgnoreCase)
                        || root.name.Contains("Skeleton", StringComparison.OrdinalIgnoreCase);

                    SerializedObject serializedVfx = new(vfx);
                    serializedVfx.FindProperty("hasarEfekti").objectReferenceValue =
                        LoadPrefab(slime ? PoisonHitPath : bossOrUndead ? DarkEffectPath : BloodEffectPath);
                    serializedVfx.FindProperty("saldırıTemasEfekti").objectReferenceValue =
                        LoadPrefab(slime ? PoisonHitPath : bossOrUndead ? DarkEffectPath : BloodEffectPath);
                    serializedVfx.FindProperty("ölümEfekti").objectReferenceValue =
                        LoadPrefab(slime ? PoisonDeathPath : DarkEffectPath);
                    serializedVfx.FindProperty("özelEfekt").objectReferenceValue =
                        slime ? LoadPrefab(ExplosionPath) : null;
                    serializedVfx.FindProperty("efektÖlçeği").floatValue = ResolveScale(root);
                    serializedVfx.ApplyModifiedPropertiesWithoutUndo();

                    ExplodingEnemy exploding = root.GetComponent<ExplodingEnemy>();
                    if (exploding != null)
                    {
                        SerializedObject serializedExplosion = new(exploding);
                        serializedExplosion.FindProperty("patlamaEfektÇarpanı").floatValue = 0.70f;
                        serializedExplosion.ApplyModifiedPropertiesWithoutUndo();
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    updated++;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Efekt animasyonu boyutları ve kompakt efekt seçimleri düzeltildi: {updated} düşman/boss prefabı.");
        }

        private static GameObject LoadPrefab(string path)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path)
                ?? throw new InvalidOperationException("Efekt prefabı bulunamadı: " + path);
        }

        private static float ResolveScale(GameObject root)
        {
            if (SpecialScales.TryGetValue(root.name, out float specialScale))
            {
                return specialScale;
            }

            return root.GetComponent<BossEnemyBrain>() != null
                || root.name.Contains("Boss", StringComparison.OrdinalIgnoreCase)
                ? 0.038f
                : 0.028f;
        }
    }
}