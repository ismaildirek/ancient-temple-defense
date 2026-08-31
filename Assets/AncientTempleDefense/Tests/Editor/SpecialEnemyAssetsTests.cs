using System.Linq;
using AncientTempleDefense.Audio;
using AncientTempleDefense.Economy;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Vfx;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AncientTempleDefense.Tests
{
    public sealed class SpecialEnemyAssetsTests
    {
        private const string PrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";

        [TestCase("BatFlyingEnemy", EnemyTargetMode.DefendersOnly, true, false)]
        [TestCase("MimicEnemy", EnemyTargetMode.NearestThreat, false, false)]
        [TestCase("RatRaiderEnemy", EnemyTargetMode.TempleOnly, false, false)]
        [TestCase("ExplodingSlimeEnemy", EnemyTargetMode.NearestThreat, false, true)]
        [TestCase("DarkWolfEnemy", EnemyTargetMode.PlayerOnly, false, false)]
        public void SpecialEnemyPrefabContainsCompleteGameplaySetup(
            string prefabName,
            EnemyTargetMode targetMode,
            bool isFlying,
            bool explodes)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
            Assert.That(prefab, Is.Not.Null, $"Prefab bulunamadı: {prefabName}");
            Assert.That(prefab.GetComponent<EnemyCombatant>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyBrain>(), Is.Not.Null);

            EnemyRoleProfile role = prefab.GetComponent<EnemyRoleProfile>();
            Assert.That(role, Is.Not.Null);
            Assert.That(role.TargetMode, Is.EqualTo(targetMode));
            Assert.That(role.IsFlying, Is.EqualTo(isFlying));

            AnimatorController controller = prefab.GetComponent<Animator>().runtimeAnimatorController as AnimatorController;
            Assert.That(controller, Is.Not.Null);
            string[] stateNames = controller.layers[0].stateMachine.states
                .Select(child => child.state.name)
                .ToArray();
            foreach (string requiredState in new[] { "Idle", "Move", "Attack1", "Attack2", "Hit", "Death" })
            {
                Assert.That(stateNames, Does.Contain(requiredState), $"{prefabName}: {requiredState} yok.");
            }

            CapsuleCollider2D collider = prefab.GetComponent<CapsuleCollider2D>();
            SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
            Assert.That(collider, Is.Not.Null);
            Assert.That(collider.isTrigger, Is.True);
            Assert.That(collider.size.x, Is.Positive);
            Assert.That(collider.size.y, Is.Positive);
            Assert.That(collider.size.x, Is.LessThanOrEqualTo(renderer.sprite.bounds.size.x + 0.01f));
            Assert.That(collider.size.y, Is.LessThanOrEqualTo(renderer.sprite.bounds.size.y + 0.01f));

            EnemyVfxController vfx = prefab.GetComponent<EnemyVfxController>();
            Assert.That(vfx, Is.Not.Null);
            Assert.That(vfx.SpawnEffectPrefab, Is.Null, "Kalabalık doğma efekti portal sunumuyla çakışmamalı.");
            Assert.That(vfx.HitEffectPrefab, Is.Not.Null);
            Assert.That(vfx.AttackEffectPrefab, Is.Not.Null);
            Assert.That(vfx.DeathEffectPrefab, Is.Not.Null);
            Assert.That(vfx.EffectScale, Is.LessThanOrEqualTo(0.055f));
            Vector2 worldColliderSize = Vector2.Scale(collider.size, prefab.transform.lossyScale);
            Assert.That(worldColliderSize.x, Is.GreaterThanOrEqualTo(0.45f));
            Assert.That(worldColliderSize.y, Is.GreaterThanOrEqualTo(0.45f));

            EnemyAudioController audio = prefab.GetComponent<EnemyAudioController>();
            Assert.That(audio, Is.Not.Null);
            Assert.That(audio.ConfiguredClips.Count(), Is.GreaterThan(0));

            EnemyLootDropper loot = prefab.GetComponent<EnemyLootDropper>();
            Assert.That(loot, Is.Not.Null);
            Assert.That(loot.CoinDropChance, Is.EqualTo(0.40f).Within(0.001f));
            Assert.That(loot.PotionDropChance, Is.EqualTo(0.20f).Within(0.001f));

            Assert.That(prefab.GetComponent<ExplodingEnemy>() != null, Is.EqualTo(explodes));
            if (explodes)
            {
                Assert.That(vfx.SpecialEffectPrefab, Is.Not.Null);
            }
        }

        [Test]
        public void MapReferencesFourLateEnemiesAndWaveNinePlayerOnlyWolf()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Map.unity", OpenSceneMode.Single);
            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            Assert.That(spawner, Is.Not.Null);
            Assert.That(spawner.WolfUnlocked(8), Is.False);
            Assert.That(spawner.WolfUnlocked(9), Is.True);
            Assert.That(spawner.WolfPrefab, Is.Not.Null);
            Assert.That(spawner.WolfPrefab.name, Is.EqualTo("DarkWolfEnemy"));
            Assert.That(
                spawner.WolfPrefab.GetComponent<EnemyRoleProfile>().TargetMode,
                Is.EqualTo(EnemyTargetMode.PlayerOnly));

            SerializedObject serialized = new(spawner);
            SerializedProperty roster = serialized.FindProperty("geçDönemDüşmanPrefabları");
            string[] names = Enumerable.Range(0, roster.arraySize)
                .Select(index => roster.GetArrayElementAtIndex(index).objectReferenceValue)
                .OfType<EnemyCombatant>()
                .Select(enemy => enemy.name)
                .ToArray();
            Assert.That(names, Does.Contain("BatFlyingEnemy"));
            Assert.That(names, Does.Contain("MimicEnemy"));
            Assert.That(names, Does.Contain("RatRaiderEnemy"));
            Assert.That(names, Does.Contain("ExplodingSlimeEnemy"));
        }
        [TestCase("RatRaiderEnemy", 2.6f)]
        [TestCase("ExplodingSlimeEnemy", 1.35f)]
        public void SmallEnemyPrefabsUseSlightlyLargerScale(string prefabName, float expectedScale)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
            Assert.That(prefab, Is.Not.Null, prefabName);
            Assert.That(prefab.transform.localScale.x, Is.EqualTo(expectedScale).Within(0.001f), prefabName);
        }
    }
}
