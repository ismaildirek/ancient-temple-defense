using AncientTempleDefense.Enemies;
using AncientTempleDefense.Scene;
using AncientTempleDefense.Vfx;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Tests
{
    public sealed class GeneratedEnvironmentAssetsTests
    {
        private const string ArtRoot = "Assets/AncientTempleDefense/Art/Generated/Environment";

        [TestCase("arena_foreground_ruins_v1.png")]
        [TestCase("enemy_shadow_breach_v2.png")]
        [TestCase("ward_seal_core_v1.png")]
        public void GeneratedEnvironmentSpriteUsesPixelArtImportSettings(string fileName)
        {
            string path = $"{ArtRoot}/{fileName}";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            Assert.That(importer, Is.Not.Null);
            Assert.That(sprite, Is.Not.Null);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.alphaIsTransparency, Is.True);
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(100f));
        }

        [Test]
        public void MapContainsEnvironmentPresentationAndSpawnResponsivePortals()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(
                "Assets/Scenes/Map.unity",
                OpenSceneMode.Single);
            GameplaySceneMarker marker = Object.FindFirstObjectByType<GameplaySceneMarker>();
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.gameObject.scene, Is.EqualTo(scene));

            Transform presentation = marker.transform.Find("EnvironmentPresentation");
            Assert.That(presentation, Is.Not.Null);
            AssertSprite(presentation, "ArenaForeground", 6);
            AssertSprite(presentation, "TempleWardSeal", 3);
            AssertSprite(presentation, "EnemyPortalLeft", 4);
            AssertSprite(presentation, "EnemyPortalRight", 4);

            SpriteRenderer leftRenderer = presentation.Find("EnemyPortalLeft").GetComponent<SpriteRenderer>();
            Assert.That(AssetDatabase.GetAssetPath(leftRenderer.sprite),
                Is.EqualTo(ArtRoot + "/enemy_shadow_breach_v2.png"));

            AmbientSpritePulse left = presentation.Find("EnemyPortalLeft").GetComponent<AmbientSpritePulse>();
            AmbientSpritePulse right = presentation.Find("EnemyPortalRight").GetComponent<AmbientSpritePulse>();
            Assert.That(left, Is.Not.Null);
            Assert.That(right, Is.Not.Null);

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            SerializedObject serialized = new(spawner);
            Assert.That(serialized.FindProperty("solDoğmaPortalı").objectReferenceValue, Is.EqualTo(left));
            Assert.That(serialized.FindProperty("sağDoğmaPortalı").objectReferenceValue, Is.EqualTo(right));
        }

        private static void AssertSprite(Transform presentation, string childName, int sortingOrder)
        {
            Transform child = presentation.Find(childName);
            Assert.That(child, Is.Not.Null, $"Environment child missing: {childName}");
            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
            Assert.That(renderer, Is.Not.Null);
            Assert.That(renderer.sprite, Is.Not.Null);
            Assert.That(renderer.sortingOrder, Is.EqualTo(sortingOrder));
        }
    }
}
