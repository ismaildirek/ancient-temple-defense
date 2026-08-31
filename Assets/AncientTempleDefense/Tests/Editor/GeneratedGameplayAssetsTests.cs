using System.Linq;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.Scene;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AncientTempleDefense.Tests
{
    public sealed class GeneratedGameplayAssetsTests
    {
        private const string GeneratedRoot = "Assets/AncientTempleDefense/Generated";

        [Test]
        public void PlayerPrefabContainsControllerAndAllPackageAnimations()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GeneratedRoot + "/Prefabs/BlackKnightPlayer.prefab");

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<BlackKnightPlayerController>(), Is.Not.Null);
            Animator animator = prefab.GetComponent<Animator>();
            Assert.That(animator, Is.Not.Null);
            Assert.That(animator.runtimeAnimatorController, Is.Not.Null);
            Assert.That(animator.runtimeAnimatorController.animationClips.Length, Is.GreaterThanOrEqualTo(34));
        }

        [Test]
        public void PlayerPrefabUsesDistinctDamageForKeysOneTwoAndFour()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GeneratedRoot + "/Prefabs/BlackKnightPlayer.prefab");
            SerializedObject player = new(prefab.GetComponent<BlackKnightPlayerController>());

            Assert.That(player.FindProperty("hafifSaldırıHasarı").intValue, Is.EqualTo(1));
            Assert.That(player.FindProperty("ağırSaldırıHasarı").intValue, Is.EqualTo(2));
            Assert.That(player.FindProperty("ultiHasarı").intValue, Is.EqualTo(3));
        }

        [TestCase("Skeleton", 7)]
[TestCase("Goblin", 6)]
        [TestCase("Mushroom", 6)]
        [TestCase("FlyingEye", 6)]
        public void EnemyPrefabContainsCompleteAnimationSet(string enemyName, int expectedClipCount)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{GeneratedRoot}/Prefabs/{enemyName}Enemy.prefab");

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyCombatant>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyBrain>(), Is.Not.Null);
            Animator animator = prefab.GetComponent<Animator>();
            Assert.That(animator.runtimeAnimatorController.animationClips.Length, Is.EqualTo(expectedClipCount));
        }

        [Test]
        public void MapSceneContainsGeneratedGameplayRootAndPlayer()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Map.unity", OpenSceneMode.Single);

            GameplaySceneMarker marker = Object.FindFirstObjectByType<GameplaySceneMarker>();
            Assert.That(marker, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<BlackKnightPlayerController>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<EnemyWaveSpawner>(), Is.Not.Null);
        }

        [Test]
        public void EveryEnemyControllerContainsHitAndDeathStates()
        {
            string[] prefabNames = { "Skeleton", "Goblin", "Mushroom", "FlyingEye" };
            foreach (string prefabName in prefabNames)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{GeneratedRoot}/Prefabs/{prefabName}Enemy.prefab");
                string[] clipNames = prefab.GetComponent<Animator>().runtimeAnimatorController.animationClips
                    .Select(clip => clip.name)
                    .ToArray();

                CollectionAssert.Contains(clipNames, "Hit");
                CollectionAssert.Contains(clipNames, "Death");
                CollectionAssert.Contains(clipNames, "Attack1");
                CollectionAssert.Contains(clipNames, "Attack2");
            }
        }
    }
}
