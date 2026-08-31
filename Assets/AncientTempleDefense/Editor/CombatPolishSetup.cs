using System;
using System.Collections.Generic;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Vfx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class CombatPolishSetup
    {
        private const string MapPath = "Assets/Scenes/Map.unity";
        private const string BloodHitPath = "Assets/100BestEffectPack/Effects/BloodEffect/BloodEffect2.prefab";
        private const string BloodAttackPath = "Assets/100BestEffectPack/Effects/BloodEffect/BloodEffect2.prefab";
        private const string BloodDeathPath = "Assets/100BestEffectPack/Effects/BloodEffect/BloodEffect5.prefab";
        private const string DarkPath = "Assets/100BestEffectPack/Effects/DarkEffect/DarkEffect5.prefab";

        private static readonly HashSet<string> SpecialEnemyNames = new(StringComparer.Ordinal)
        {
            "BatFlyingEnemy",
            "MimicEnemy",
            "RatRaiderEnemy",
            "ExplodingSlimeEnemy",
            "DarkWolfEnemy"
        };

        [MenuItem("Tools/Ancient Temple Defense/Savaş ve Efekt Cilası")]
        public static void Configure()
        {
            SpecialEnemySetup.Configure();

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MapPath, OpenSceneMode.Single);
            EnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EnemyWaveSpawner>();
            if (spawner == null)
            {
                throw new InvalidOperationException("Map sahnesinde EnemyWaveSpawner bulunamadı.");
            }

            HashSet<string> prefabPaths = CollectEnemyPrefabPaths(spawner);
            int configuredCount = 0;
            foreach (string prefabPath in prefabPaths)
            {
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabAsset == null || SpecialEnemyNames.Contains(prefabAsset.name))
                {
                    continue;
                }

                ConfigurePrefab(prefabPath, prefabAsset.name);
                configuredCount++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Savaş cilası tamamlandı: {configuredCount} klasik düşman/boss ve 5 özel düşman güncellendi.");
        }

        private static HashSet<string> CollectEnemyPrefabPaths(EnemyWaveSpawner spawner)
        {
            SerializedObject serialized = new(spawner);
            HashSet<string> paths = new(StringComparer.OrdinalIgnoreCase);
            AddArrayPaths(serialized.FindProperty("düşmanPrefabları"), paths);
            AddArrayPaths(serialized.FindProperty("geçDönemDüşmanPrefabları"), paths);
            AddObjectPath(serialized.FindProperty("birinciBossPrefabı"), paths);
            AddObjectPath(serialized.FindProperty("ikinciBossPrefabı"), paths);
            return paths;
        }

        private static void AddArrayPaths(SerializedProperty array, ISet<string> paths)
        {
            if (array == null || !array.isArray)
            {
                return;
            }

            for (int index = 0; index < array.arraySize; index++)
            {
                AddObjectPath(array.GetArrayElementAtIndex(index), paths);
            }
        }

        private static void AddObjectPath(SerializedProperty property, ISet<string> paths)
        {
            UnityEngine.Object reference = property?.objectReferenceValue;
            string path = reference != null ? AssetDatabase.GetAssetPath(reference) : string.Empty;
            if (!string.IsNullOrWhiteSpace(path) && path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                paths.Add(path);
            }
        }

        private static void ConfigurePrefab(string prefabPath, string prefabName)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (root.GetComponent<EnemyCombatant>() == null)
                {
                    return;
                }

                EnemyVfxController vfx = root.GetComponent<EnemyVfxController>();
                if (vfx == null)
                {
                    vfx = root.AddComponent<EnemyVfxController>();
                }

                bool boss = root.GetComponent<BossEnemyBrain>() != null
                    || prefabName.Contains("Boss", StringComparison.OrdinalIgnoreCase);
                bool undead = prefabName.Contains("Skeleton", StringComparison.OrdinalIgnoreCase);

                SerializedObject serialized = new(vfx);
                serialized.FindProperty("doğmaEfekti").objectReferenceValue = null;
                serialized.FindProperty("hasarEfekti").objectReferenceValue = LoadPrefab(undead || boss ? DarkPath : BloodHitPath);
                serialized.FindProperty("saldırıTemasEfekti").objectReferenceValue = LoadPrefab(boss ? DarkPath : BloodAttackPath);
                serialized.FindProperty("ölümEfekti").objectReferenceValue = LoadPrefab(DarkPath);
                serialized.FindProperty("özelEfekt").objectReferenceValue = null;
                serialized.FindProperty("efektÖlçeği").floatValue = boss ? 0.038f : 0.028f;
                serialized.FindProperty("efektYOfseti").floatValue = boss ? 0.18f : 0.08f;
                serialized.FindProperty("efektÖmrü").floatValue = boss ? 0.95f : 0.78f;
                serialized.FindProperty("particleSıralaması").intValue = 12;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static GameObject LoadPrefab(string path)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path)
                ?? throw new InvalidOperationException("Efekt prefabı bulunamadı: " + path);
        }
    }
}