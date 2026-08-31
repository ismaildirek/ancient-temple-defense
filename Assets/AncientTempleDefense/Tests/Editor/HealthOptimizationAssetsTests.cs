using System.Reflection;
using AncientTempleDefense.Allies;
using AncientTempleDefense.Economy;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AncientTempleDefense.Tests
{
    public sealed class HealthOptimizationAssetsTests
    {
        private const string PrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";

        [Test]
        public void PlayerCombatUsesReusablePhysicsBuffer()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + "/BlackKnightPlayer.prefab");
            Assert.That(prefab, Is.Not.Null);

            BlackKnightPlayerController controller = prefab.GetComponent<BlackKnightPlayerController>();
            FieldInfo bufferField = typeof(BlackKnightPlayerController).GetField(
                "_attackHits",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(bufferField, Is.Not.Null);
            Collider2D[] buffer = (Collider2D[])bufferField.GetValue(controller);
            Assert.That(buffer, Is.Not.Null);
            Assert.That(buffer.Length, Is.GreaterThanOrEqualTo(16));
        }

        [TestCase("CoinPickup", PickupType.Coin, 1)]
        [TestCase("HealthPotionPickup", PickupType.HealthPotion, 50)]
        public void PickupPrefabsHaveExpectedValueAndFiniteLifetime(
            string prefabName,
            PickupType expectedType,
            int expectedValue)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
            Assert.That(prefab, Is.Not.Null);

            WorldPickup pickup = prefab.GetComponent<WorldPickup>();
            Assert.That(pickup, Is.Not.Null);
            Assert.That(pickup.Type, Is.EqualTo(expectedType));
            Assert.That(pickup.Value, Is.EqualTo(expectedValue));

            SerializedObject serialized = new(pickup);
            SerializedProperty lifetime = serialized.FindProperty("dunyadaKalmaSuresi");
            Assert.That(lifetime, Is.Not.Null);
            Assert.That(lifetime.floatValue, Is.GreaterThanOrEqualTo(5f));
        }

        [Test]
        public void EnemyPrefabsUseNonBlockingCollidersAndBoundLootReferences()
        {
            string[] prefabNames =
            {
                "SkeletonEnemy", "GoblinEnemy", "MushroomEnemy", "FlyingEyeEnemy",
                "NewEnemy1", "NewEnemy2", "NewEnemy3", "NewEnemy4", "NewEnemy5",
                "Boss1Enemy", "Boss2Enemy"
            };

            foreach (string prefabName in prefabNames)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
                Assert.That(prefab, Is.Not.Null, prefabName);

                Collider2D collider = prefab.GetComponent<Collider2D>();
                Assert.That(collider, Is.Not.Null, prefabName);
                Assert.That(collider.isTrigger, Is.True, prefabName);

                EnemyLootDropper dropper = prefab.GetComponent<EnemyLootDropper>();
                Assert.That(dropper, Is.Not.Null, prefabName);
                Assert.That(dropper.CoinDropChance, Is.EqualTo(0.40f).Within(0.001f), prefabName);
                Assert.That(dropper.PotionDropChance, Is.EqualTo(0.20f).Within(0.001f), prefabName);

                SerializedObject serialized = new(dropper);
                Assert.That(serialized.FindProperty("coinPrefabi").objectReferenceValue, Is.Not.Null, prefabName);
                Assert.That(serialized.FindProperty("canIksiriPrefabi").objectReferenceValue, Is.Not.Null, prefabName);
            }
        }

        [Test]
        public void AllyPrefabsAreAnimatedTriggerDefenders()
        {
            string[] prefabNames = { "MartialHeroAlly", "HeroKnightAlly" };
            foreach (string prefabName in prefabNames)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
                Assert.That(prefab, Is.Not.Null, prefabName);
                Assert.That(prefab.GetComponent<FriendlyDefender>(), Is.Not.Null, prefabName);
                Assert.That(prefab.GetComponent<Collider2D>().isTrigger, Is.True, prefabName);

                Animator animator = prefab.GetComponent<Animator>();
                Assert.That(animator.runtimeAnimatorController, Is.Not.Null, prefabName);
                Assert.That(animator.runtimeAnimatorController.animationClips.Length, Is.GreaterThanOrEqualTo(6), prefabName);
            }
        }

        [Test]
        public void DefenseShopSceneReferencesAndPricesAreComplete()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Map.unity", OpenSceneMode.Single);

            DefenseShopPanel shop = Object.FindFirstObjectByType<DefenseShopPanel>();
            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            Assert.That(shop, Is.Not.Null);
            Assert.That(spawner, Is.Not.Null);

            SerializedObject shopData = new(shop);
            Assert.That(shopData.FindProperty("martialHeroPrefabi").objectReferenceValue, Is.Not.Null);
            Assert.That(shopData.FindProperty("heroKnightPrefabi").objectReferenceValue, Is.Not.Null);
            Assert.That(shopData.FindProperty("askerFiyati").intValue, Is.EqualTo(20));
            Assert.That(shopData.FindProperty("okcuFiyati").intValue, Is.EqualTo(50));
            Assert.That(shopData.FindProperty("buyucuFiyati").intValue, Is.EqualTo(60));
            Assert.That(shopData.FindProperty("enFazlaDost").intValue, Is.EqualTo(3));

            SerializedObject waveData = new(spawner);
            Assert.That(waveData.FindProperty("savunmaMagazasiPaneli").objectReferenceValue, Is.EqualTo(shop));
        }
        [TestCase("MartialHeroAlly", 3.1f, 180)]
        [TestCase("HeroKnightAlly", 1.45f, 240)]
        public void AllyPrefabsUseReadableScaleAndDurableHealth(
            string prefabName,
            float expectedScale,
            int expectedHealth)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
            Assert.That(prefab, Is.Not.Null, prefabName);
            Assert.That(prefab.transform.localScale.x, Is.EqualTo(expectedScale).Within(0.001f), prefabName);
            Assert.That(prefab.GetComponent<FriendlyDefender>().MaximumHealth, Is.EqualTo(expectedHealth), prefabName);
        }
        [TestCase("Boss1Enemy", true)]
        [TestCase("Boss2Enemy", false)]
        public void BossPrefabsPreserveTheirSourceSpriteDirection(string prefabName, bool expectedFacesLeft)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
            Assert.That(prefab, Is.Not.Null, prefabName);

            BossEnemyBrain brain = prefab.GetComponent<BossEnemyBrain>();
            Assert.That(brain, Is.Not.Null, prefabName);
            SerializedObject serialized = new(brain);
            Assert.That(
                serialized.FindProperty("kaynakVarsayilanYonuSola").boolValue,
                Is.EqualTo(expectedFacesLeft),
                prefabName);
        }
    }
}
