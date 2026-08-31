using AncientTempleDefense.Enemies;
using AncientTempleDefense.UI;
using AncientTempleDefense.Vfx;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AncientTempleDefense.Tests
{
    public sealed class CombatPolishTests
    {
        [Test]
        public void InstructionPanelStartsBelowTopHudBandAtCommonResolutions()
        {
            Rect fullHd = GameInstructionsOverlay.CalculatePanelRect(1920, 1080);
            Rect ultrawide = GameInstructionsOverlay.CalculatePanelRect(3840, 1080);
            Rect hd = GameInstructionsOverlay.CalculatePanelRect(1280, 720);

            Assert.That(fullHd.yMin, Is.GreaterThanOrEqualTo(138f));
            Assert.That(ultrawide.yMin, Is.GreaterThanOrEqualTo(138f));
            Assert.That(hd.yMin, Is.GreaterThanOrEqualTo(94f));
            Assert.That(fullHd.xMax, Is.LessThanOrEqualTo(1920f));
            Assert.That(hd.xMax, Is.LessThanOrEqualTo(1280f));
        }

        [Test]
        public void EveryWaveEnemyUsesSmallShortVfxProfile()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Map.unity", OpenSceneMode.Single);
            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            Assert.That(spawner, Is.Not.Null);

            SerializedObject serialized = new(spawner);
            AssertVfxArray(serialized.FindProperty("düşmanPrefabları"));
            AssertVfxArray(serialized.FindProperty("geçDönemDüşmanPrefabları"));
            AssertVfx(serialized.FindProperty("birinciBossPrefabı").objectReferenceValue as EnemyCombatant);
            AssertVfx(serialized.FindProperty("ikinciBossPrefabı").objectReferenceValue as EnemyCombatant);
        }

        private static void AssertVfxArray(SerializedProperty array)
        {
            for (int index = 0; index < array.arraySize; index++)
            {
                AssertVfx(array.GetArrayElementAtIndex(index).objectReferenceValue as EnemyCombatant);
            }
        }

        private static void AssertVfx(EnemyCombatant enemy)
        {
            Assert.That(enemy, Is.Not.Null);
            EnemyVfxController vfx = enemy.GetComponent<EnemyVfxController>();
            Assert.That(vfx, Is.Not.Null, enemy.name);
            Assert.That(vfx.SpawnEffectPrefab, Is.Null, enemy.name);
            Assert.That(vfx.HitEffectPrefab, Is.Not.Null, enemy.name);
            Assert.That(vfx.AttackEffectPrefab, Is.Not.Null, enemy.name);
            Assert.That(vfx.DeathEffectPrefab, Is.Not.Null, enemy.name);
            Assert.That(vfx.EffectScale, Is.InRange(0.02f, 0.04f), enemy.name);
            ExplodingEnemy exploding = enemy.GetComponent<ExplodingEnemy>();
            if (exploding != null)
            {
                Assert.That(exploding.ExplosionEffectMultiplier, Is.LessThanOrEqualTo(0.70f), enemy.name);
                Assert.That(vfx.SpecialEffectPrefab.name, Is.EqualTo("ExplosionEffect4"), enemy.name);
                Assert.That(vfx.DeathEffectPrefab.name, Is.EqualTo("PoisonEffect5"), enemy.name);
            }
            else
            {
                Assert.That(vfx.DeathEffectPrefab.name, Is.EqualTo("DarkEffect5"), enemy.name);
            }
        }
    }
}