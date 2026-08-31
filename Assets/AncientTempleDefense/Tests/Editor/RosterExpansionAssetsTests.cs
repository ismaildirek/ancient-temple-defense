using System.Linq;
using AncientTempleDefense.Allies;
using AncientTempleDefense.Audio;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AncientTempleDefense.Tests
{
    public sealed class RosterExpansionAssetsTests
    {
        private const string Prefabs = "Assets/AncientTempleDefense/Generated/Prefabs";

        [TestCase("LowSoldierLight")]
        [TestCase("LowSoldierHeavy")]
        public void LowSoldiersAreWeakerCompleteAndAudible(string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Prefabs}/{name}.prefab");
            Assert.That(prefab, Is.Not.Null);
            FriendlyDefender defender = prefab.GetComponent<FriendlyDefender>();
            Assert.That(defender, Is.Not.Null);
            Assert.That(defender.MaximumHealth, Is.LessThan(180));
            string[] states = prefab.GetComponent<Animator>().runtimeAnimatorController.animationClips.Select(c => c.name).ToArray();
            foreach (string state in new[] { "Idle", "Run", "Attack1", "Attack2", "Hit", "Death" })
                Assert.That(states, Does.Contain(state), name);
            Assert.That(prefab.GetComponent<EnemyAudioController>().ConfiguredClips.Count(), Is.GreaterThan(0));
        }

        [Test]
        public void SceneContainsCheapSoldiersAndFourOrderedBosses()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Map.unity", OpenSceneMode.Single);
            DefenseShopPanel shop = Object.FindFirstObjectByType<DefenseShopPanel>();
            SerializedObject shopData = new(shop);
            Assert.That(shop.LowSoldierPrice, Is.EqualTo(10));
            Assert.That(shopData.FindProperty("dusukAskerPrefabları").arraySize, Is.EqualTo(2));

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            Assert.That(spawner.BossPrefabForWave(7).GetComponent<BossEnemyBrain>().BossTier, Is.EqualTo(1));
            Assert.That(spawner.BossPrefabForWave(12).GetComponent<BossEnemyBrain>().BossTier, Is.EqualTo(2));
            Assert.That(spawner.BossPrefabForWave(17).GetComponent<BossEnemyBrain>().BossTier, Is.EqualTo(3));
            Assert.That(spawner.BossPrefabForWave(22).GetComponent<BossEnemyBrain>().BossTier, Is.EqualTo(4));
        }

        [TestCase("SkeletonEnemy", 2.5f)]
        [TestCase("NewEnemy3", 2.65f)]
        [TestCase("DarkWolfEnemy", 1.4f)]
        public void RepairedEnemiesUseIntendedScaleAndVisiblePixelGround(string name, float scale)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Prefabs}/{name}.prefab");
            Assert.That(prefab.transform.localScale.x, Is.EqualTo(scale).Within(.001f));
            Sprite sprite = prefab.GetComponent<SpriteRenderer>().sprite;
            CapsuleCollider2D collider = prefab.GetComponent<CapsuleCollider2D>();
            float visibleBottom = sprite.vertices.Min(vertex => vertex.y);
            float colliderBottom = collider.offset.y - collider.size.y * .5f;
            Assert.That(colliderBottom, Is.EqualTo(visibleBottom).Within(.001f));
        }
    }
}
