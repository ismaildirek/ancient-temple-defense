using System.Linq;
using System.Reflection;
using AncientTempleDefense.Audio;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.Progression;
using AncientTempleDefense.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace AncientTempleDefense.Tests
{
public sealed class WaveProgressionTests
    {
        private const string MapScenePath = "Assets/Scenes/Map.unity";
        private const string FontPath = "Assets/Thaleah_PixelFont/Materials/ThaleahFat_TTF.ttf";

        [Test]
        public void WaveScalingRaisesCountHealthDamageAndAttackSpeed()
        {
            Assert.That(WaveScaling.EnemyCount(1, 4, 2), Is.EqualTo(4));
            Assert.That(WaveScaling.EnemyHealth(1, 3, 0.12f), Is.EqualTo(3));
            Assert.That(WaveScaling.EnemyDamage(1, 8, 0.10f), Is.EqualTo(8));
            Assert.That(WaveScaling.AttackSpeedMultiplier(1, 0.04f), Is.EqualTo(1f));

            Assert.That(WaveScaling.EnemyCount(5, 4, 2), Is.EqualTo(12));
            Assert.That(WaveScaling.EnemyHealth(5, 3, 0.12f), Is.EqualTo(5));
            Assert.That(WaveScaling.EnemyDamage(5, 8, 0.10f), Is.EqualTo(12));
            Assert.That(WaveScaling.AttackSpeedMultiplier(5, 0.04f), Is.EqualTo(1.16f).Within(0.001f));
        }

        [Test]
        public void UpgradeCatalogReturnsThreeUniqueDeterministicChoices()
        {
            PlayerUpgradeCard[] first = PlayerUpgradeCatalog.CreateChoices(5);
            PlayerUpgradeCard[] second = PlayerUpgradeCatalog.CreateChoices(5);

            Assert.That(first, Has.Length.EqualTo(3));
            Assert.That(first.Select(card => card.Type).Distinct().Count(), Is.EqualTo(3));
            CollectionAssert.AreEqual(
                first.Select(card => card.Type).ToArray(),
                second.Select(card => card.Type).ToArray());
        }

        [Test]
        public void InspectorUsesTurkishLabels()
        {
            AssertInspectorName(typeof(BlackKnightPlayerController), "hareketHızı", "Hareket H\u0131z\u0131");
            AssertInspectorName(typeof(BlackKnightPlayerController), "hafifSaldırıHasarı", "Hafif Sald\u0131r\u0131 Hasar\u0131");
            AssertInspectorName(typeof(BlackKnightSwordAudio), "hafifSaldırıSesleri", "Hafif Sald\u0131r\u0131 Sesleri");
            AssertInspectorName(typeof(EnemyWaveSpawner), "waveBaşınaCanArtışı", "Wave Ba\u015f\u0131na Can Art\u0131\u015f\u0131");
        }

        [Test]
public void MapContainsWavePanelPlayerHealthAndThaleahFont()
        {
            EditorSceneManager.OpenScene(MapScenePath, OpenSceneMode.Single);

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            WaveUpgradePanel panel = Object.FindFirstObjectByType<WaveUpgradePanel>();
            PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
            Font expectedFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);

            Assert.That(spawner, Is.Not.Null);
            Assert.That(panel, Is.Not.Null);
            Assert.That(health, Is.Not.Null);
            Assert.That(expectedFont, Is.Not.Null);
            Assert.That(panel.PixelFont, Is.SameAs(expectedFont));
        }

        private static void AssertInspectorName(System.Type type, string fieldName, string expected)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            InspectorNameAttribute label = field.GetCustomAttribute<InspectorNameAttribute>();
            Assert.That(label, Is.Not.Null);
            Assert.That(label.displayName, Is.EqualTo(expected));
        }
    }
}
