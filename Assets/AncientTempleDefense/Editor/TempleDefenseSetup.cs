using System;
using AncientTempleDefense.Temple;
using AncientTempleDefense.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class TempleDefenseSetup
    {
        private const string MapScenePath = "Assets/Scenes/Map.unity";

        [MenuItem("Tools/Ancient Temple Defense/Configure Temple Health And Boss Phases")]
        public static void Configure()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MapScenePath, OpenSceneMode.Single);
            GameObject templeObject = GameObject.Find("ancient_temple_base")
                ?? throw new InvalidOperationException("Map sahnesinde ancient_temple_base bulunamadı.");
            SpriteRenderer templeRenderer = templeObject.GetComponent<SpriteRenderer>()
                ?? throw new InvalidOperationException("Tapınak SpriteRenderer bileşeni bulunamadı.");

            TempleHealth templeHealth = templeObject.GetComponent<TempleHealth>()
                ?? templeObject.AddComponent<TempleHealth>();
            SpriteRenderer wardRenderer = GameObject.Find("TempleWardSeal")?.GetComponent<SpriteRenderer>();

            SerializedObject templeSerialized = new(templeHealth);
            templeSerialized.FindProperty("azamiCan").intValue = 2000;
            templeSerialized.FindProperty("hasarlıAşamaEşiği").floatValue = 0.66f;
            templeSerialized.FindProperty("kritikAşamaEşiği").floatValue = 0.33f;
            templeSerialized.FindProperty("anaGörsel").objectReferenceValue = templeRenderer;
            templeSerialized.FindProperty("mühürGörseli").objectReferenceValue = wardRenderer;
            templeSerialized.ApplyModifiedPropertiesWithoutUndo();

            WaveUpgradePanel panel = UnityEngine.Object.FindFirstObjectByType<WaveUpgradePanel>()
                ?? throw new InvalidOperationException("Map sahnesinde WaveUpgradePanel bulunamadı.");
            SerializedObject panelSerialized = new(panel);
            panelSerialized.FindProperty("tapınakCanı").objectReferenceValue = templeHealth;
            panelSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, MapScenePath))
            {
                throw new InvalidOperationException("Tapınak savunma sistemi Map sahnesine kaydedilemedi.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Tapınak canı, üç hasar görünümü ve boss ikinci faz sistemi yapılandırıldı.");
        }
    }
}
