using System;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class WaveProgressionSetup
    {
        private const string PlayerPrefabPath = "Assets/AncientTempleDefense/Generated/Prefabs/BlackKnightPlayer.prefab";
        private const string FontPath = "Assets/Thaleah_PixelFont/Materials/ThaleahFat_TTF.ttf";
        private const string MapScenePath = "Assets/Scenes/Map.unity";

        [MenuItem("Tools/Ancient Temple Defense/Configure Wave Progression")]
        public static void Configure()
        {
            ConfigurePlayerPrefab();
            ConfigureMapScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Wave ve yükseltme kartı sistemi tamamlandı.");
        }

        private static void ConfigurePlayerPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath) == null)
            {
                throw new InvalidOperationException($"Oyuncu prefabı bulunamadı: {PlayerPrefabPath}");
            }

            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                if (root.GetComponent<PlayerHealth>() == null)
                {
                    root.AddComponent<PlayerHealth>();
                }

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureMapScene()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MapScenePath, OpenSceneMode.Single);
            EnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EnemyWaveSpawner>();
            BlackKnightPlayerController player = UnityEngine.Object.FindFirstObjectByType<BlackKnightPlayerController>();
            if (spawner == null || player == null)
            {
                throw new InvalidOperationException("Map sahnesinde wave sistemi veya oyuncu bulunamadı.");
            }

            PlayerHealth health = player.GetComponent<PlayerHealth>() ?? player.gameObject.AddComponent<PlayerHealth>();
            WaveUpgradePanel panel = spawner.GetComponent<WaveUpgradePanel>() ?? spawner.gameObject.AddComponent<WaveUpgradePanel>();
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath)
                ?? throw new InvalidOperationException($"Thaleah fontu bulunamadı: {FontPath}");

            SerializedObject panelObject = new(panel);
            panelObject.FindProperty("pikselYazıTipi").objectReferenceValue = font;
            panelObject.FindProperty("oyuncu").objectReferenceValue = player;
            panelObject.FindProperty("oyuncuCanı").objectReferenceValue = health;
            panelObject.FindProperty("waveSistemi").objectReferenceValue = spawner;
            panelObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject spawnerObject = new(spawner);
            spawnerObject.FindProperty("başlangıçWave").intValue = 1;
            spawnerObject.FindProperty("ilkWaveDüşmanSayısı").intValue = 4;
            spawnerObject.FindProperty("waveBaşınaEkDüşman").intValue = 2;
            spawnerObject.FindProperty("düşmanDoğmaAralığı").floatValue = 0.45f;
            spawnerObject.FindProperty("waveArasıBekleme").floatValue = 2f;
            spawnerObject.FindProperty("aynıAndaEnFazlaDüşman").intValue = 8;
            spawnerObject.FindProperty("kaçWavedeBirKart").intValue = 5;
            spawnerObject.FindProperty("ilkDüşmanCanı").intValue = 3;
            spawnerObject.FindProperty("waveBaşınaCanArtışı").floatValue = 0.12f;
            spawnerObject.FindProperty("ilkDüşmanHasarı").intValue = 8;
            spawnerObject.FindProperty("waveBaşınaHasarArtışı").floatValue = 0.10f;
            spawnerObject.FindProperty("waveBaşınaVuruşHızıArtışı").floatValue = 0.04f;
            spawnerObject.FindProperty("yükseltmeKartıPaneli").objectReferenceValue = panel;
            spawnerObject.ApplyModifiedPropertiesWithoutUndo();

            GameInstructionsOverlay instructions = UnityEngine.Object.FindFirstObjectByType<GameInstructionsOverlay>();
            if (instructions != null)
            {
                SerializedObject instructionsObject = new(instructions);
                instructionsObject.FindProperty("pikselYazıTipi").objectReferenceValue = font;
                instructionsObject.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, MapScenePath))
            {
                throw new InvalidOperationException("Wave sistemi Map sahnesine kaydedilemedi.");
            }
        }
    }
}
