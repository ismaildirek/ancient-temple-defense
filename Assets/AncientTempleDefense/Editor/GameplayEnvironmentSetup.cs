using System;
using System.IO;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Scene;
using AncientTempleDefense.Vfx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class GameplayEnvironmentSetup
    {
        private const string ArtRoot = "Assets/AncientTempleDefense/Art/Generated/Environment";
        private const string ForegroundPath = ArtRoot + "/arena_foreground_ruins_v1.png";
        private const string PortalPath = ArtRoot + "/enemy_shadow_breach_v2.png";
        private const string WardPath = ArtRoot + "/ward_seal_core_v1.png";
        private const string MapScenePath = "Assets/Scenes/Map.unity";

        [MenuItem("Tools/Ancient Temple Defense/Configure Environment Presentation")]
        public static void Configure()
        {
            ConfigureSpriteImporter(ForegroundPath, new Vector2(0.5f, 0f));
            ConfigureSpriteImporter(PortalPath, new Vector2(0.5f, 0f));
            ConfigureSpriteImporter(WardPath, new Vector2(0.5f, 0.5f));

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MapScenePath, OpenSceneMode.Single);
            GameplaySceneMarker marker = UnityEngine.Object.FindFirstObjectByType<GameplaySceneMarker>();
            if (marker == null || marker.gameObject.scene != scene)
            {
                throw new InvalidOperationException("Map sahnesinde uretilmis Gameplay kok nesnesi bulunamadi.");
            }

            Transform existing = marker.transform.Find("EnvironmentPresentation");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject presentation = new("EnvironmentPresentation");
            presentation.transform.SetParent(marker.transform, false);

            CreateSpriteObject(
                presentation.transform,
                "ArenaForeground",
                LoadSprite(ForegroundPath),
                new Vector3(1.5f, -7.8f, 0f),
                new Vector3(1.77f, 1.77f, 1f),
                6,
                false);

            GameObject ward = CreateSpriteObject(
                presentation.transform,
                "TempleWardSeal",
                LoadSprite(WardPath),
                new Vector3(0.3f, -1.05f, 0f),
                Vector3.one * 0.12f,
                3,
                false);
            ConfigurePulse(ward.AddComponent<AmbientSpritePulse>(), 0.45f, 0.022f, 0.05f, 0f, 0.24f, 0.08f);

            GameObject leftPortal = CreateSpriteObject(
                presentation.transform,
                "EnemyPortalLeft",
                LoadSprite(PortalPath),
                new Vector3(-17f, -4.12f, 0f),
                Vector3.one * 0.30f,
                4,
                false);
            AmbientSpritePulse leftPulse = leftPortal.AddComponent<AmbientSpritePulse>();
            ConfigurePulse(leftPulse, 0.42f, 0.015f, 0.04f, 0f, 0.16f, 0.08f);

            GameObject rightPortal = CreateSpriteObject(
                presentation.transform,
                "EnemyPortalRight",
                LoadSprite(PortalPath),
                new Vector3(18f, -4.12f, 0f),
                Vector3.one * 0.30f,
                4,
                true);
            AmbientSpritePulse rightPulse = rightPortal.AddComponent<AmbientSpritePulse>();
            ConfigurePulse(rightPulse, 0.42f, 0.015f, 0.04f, 0.5f, 0.16f, 0.08f);

            EnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EnemyWaveSpawner>();
            if (spawner == null)
            {
                throw new InvalidOperationException("Map sahnesinde EnemyWaveSpawner bulunamadi.");
            }

            SerializedObject spawnerObject = new(spawner);
            spawnerObject.FindProperty("solDoğmaPortalı").objectReferenceValue = leftPulse;
            spawnerObject.FindProperty("sağDoğmaPortalı").objectReferenceValue = rightPulse;
            spawnerObject.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
if (!EditorSceneManager.SaveScene(scene, MapScenePath))
            {
                throw new InvalidOperationException("Map sahnesi yeni environment assetleriyle kaydedilemedi.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Ancient Temple Defense environment sunumu tamamlandi.");
        }

        [MenuItem("Tools/Ancient Temple Defense/Capture Environment Preview")]
        public static void CaptureEnvironmentPreview()
        {
            EditorSceneManager.OpenScene(MapScenePath, OpenSceneMode.Single);
            Camera camera = Camera.main;
            if (camera == null)
            {
                throw new InvalidOperationException("Onizleme icin Main Camera bulunamadi.");
            }

            string outputPath = Path.GetFullPath("TestResults/EnvironmentPreview.png");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)
                ?? throw new InvalidOperationException("Onizleme klasoru belirlenemedi."));

            Vector3 originalPosition = camera.transform.position;
            RenderTexture originalTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture target = new(3840, 1080, 24, RenderTextureFormat.ARGB32);
            Texture2D image = new(3840, 1080, TextureFormat.RGBA32, false);

            try
            {
                camera.transform.position = new Vector3(0.5f, 0f, originalPosition.z);
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0f, 0f, target.width, target.height), 0, 0);
                image.Apply(false, false);
                File.WriteAllBytes(outputPath, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = originalTarget;
                camera.transform.position = originalPosition;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(image);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }

            Debug.Log($"Environment onizlemesi kaydedildi: {outputPath}");
        }

        private static void ConfigureSpriteImporter(string path, Vector2 pivot)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter
                ?? throw new InvalidOperationException($"TextureImporter bulunamadi: {path}");

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.spritePivot = pivot;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path)
                ?? throw new InvalidOperationException($"Sprite bulunamadi: {path}");
        }

        private static GameObject CreateSpriteObject(
            Transform parent,
            string name,
            Sprite sprite,
            Vector3 localPosition,
            Vector3 localScale,
            int sortingOrder,
            bool flipX)
        {
            GameObject gameObject = new(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localScale = localScale;

            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.flipX = flipX;
            return gameObject;
        }

        private static void ConfigurePulse(
            AmbientSpritePulse pulse,
            float cyclesPerSecond,
            float scaleAmplitude,
            float alphaAmplitude,
            float phaseOffset,
            float burstScaleBonus,
            float burstBrightnessBonus)
        {
            SerializedObject serialized = new(pulse);
            serialized.FindProperty("saniyedekiDöngü").floatValue = cyclesPerSecond;
            serialized.FindProperty("ölçekGenliği").floatValue = scaleAmplitude;
            serialized.FindProperty("saydamlıkGenliği").floatValue = alphaAmplitude;
            serialized.FindProperty("fazKayması").floatValue = phaseOffset;
            serialized.FindProperty("parlamaÖlçekBonusu").floatValue = burstScaleBonus;
            serialized.FindProperty("parlamaParlaklıkBonusu").floatValue = burstBrightnessBonus;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }}
