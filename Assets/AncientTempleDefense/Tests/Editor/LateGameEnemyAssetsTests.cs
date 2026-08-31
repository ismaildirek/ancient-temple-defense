using System.Linq;
using AncientTempleDefense.Audio;
using AncientTempleDefense.Enemies;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AncientTempleDefense.Tests
{
    public sealed class LateGameEnemyAssetsTests
    {
        private const string PrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";

        [TestCase("NewEnemy1", "Attack1", "Attack2")]
        [TestCase("NewEnemy2", "Attack1", "Shield")]
        [TestCase("NewEnemy3", "Attack1", "Attack3")]
        [TestCase("NewEnemy4", "Attack1", "Attack2")]
        [TestCase("NewEnemy5", "Attack1", "Attack2")]
        public void NewEnemyPrefabsContainCombatAnimationAndAudio(
            string prefabName,
            string requiredStateOne,
            string requiredStateTwo)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
            Assert.That(prefab, Is.Not.Null, $"Prefab bulunamadı: {prefabName}");
            Assert.That(prefab.GetComponent<EnemyCombatant>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyBrain>(), Is.Not.Null);

            string[] clipNames = prefab.GetComponent<Animator>().runtimeAnimatorController.animationClips
                .Select(clip => clip.name)
                .ToArray();
            Assert.That(clipNames, Does.Contain("Idle"));
            Assert.That(clipNames, Does.Contain("Move"));
            Assert.That(clipNames, Does.Contain("Hit"));
            Assert.That(clipNames, Does.Contain("Death"));
            Assert.That(clipNames, Does.Contain(requiredStateOne));
            Assert.That(clipNames, Does.Contain(requiredStateTwo));

            EnemyAudioController audio = prefab.GetComponent<EnemyAudioController>();
            Assert.That(audio, Is.Not.Null);
            Assert.That(audio.ConfiguredClips.Count(), Is.GreaterThan(0));
        }

        [Test]
        public void BossTwoIsConfiguredStrongerAndBothBossesUseSpecialAnimationsAndAudio()
        {
            GameObject bossOnePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/Boss1Enemy.prefab");
            GameObject bossTwoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/Boss2Enemy.prefab");
            Assert.That(bossOnePrefab, Is.Not.Null);
            Assert.That(bossTwoPrefab, Is.Not.Null);

            BossEnemyBrain bossOne = bossOnePrefab.GetComponent<BossEnemyBrain>();
            BossEnemyBrain bossTwo = bossTwoPrefab.GetComponent<BossEnemyBrain>();
            Assert.That(bossOne, Is.Not.Null);
            Assert.That(bossTwo, Is.Not.Null);
            Assert.That(bossOne.BossTier, Is.EqualTo(1));
            Assert.That(bossTwo.BossTier, Is.EqualTo(2));
            Assert.That(bossTwo.HeavyDamageMultiplier, Is.GreaterThan(bossOne.HeavyDamageMultiplier));
            Assert.That(bossTwo.SpecialDamageMultiplier, Is.GreaterThan(bossOne.SpecialDamageMultiplier));

            string[] bossOneClips = bossOnePrefab.GetComponent<Animator>().runtimeAnimatorController.animationClips
                .Select(clip => clip.name)
                .ToArray();
            string[] bossTwoClips = bossTwoPrefab.GetComponent<Animator>().runtimeAnimatorController.animationClips
                .Select(clip => clip.name)
                .ToArray();
            Assert.That(bossOneClips, Does.Contain("Attack"));
            Assert.That(bossOneClips, Does.Contain("Cast"));
            Assert.That(bossOneClips, Does.Contain("Spell"));
            Assert.That(bossTwoClips, Does.Contain("Attack1"));
            Assert.That(bossTwoClips, Does.Contain("Attack2"));
            Assert.That(bossTwoClips, Does.Contain("Jump"));
            Assert.That(bossTwoClips, Does.Contain("Special2"));

            Assert.That(bossOnePrefab.GetComponent<EnemyAudioController>().ConfiguredClips.Count(), Is.GreaterThan(0));
            Assert.That(bossTwoPrefab.GetComponent<EnemyAudioController>().ConfiguredClips.Count(), Is.GreaterThan(0));

            SerializedObject bossOneAudio = new(bossOnePrefab.GetComponent<EnemyAudioController>());
            SerializedObject bossTwoAudio = new(bossTwoPrefab.GetComponent<EnemyAudioController>());
            Assert.That(
                bossOneAudio.FindProperty("özelSaldırıSesleri").FindPropertyRelative("sesKlipleri").arraySize,
                Is.GreaterThan(0));
            Assert.That(
                bossTwoAudio.FindProperty("güçlüÖzelSaldırıSesleri").FindPropertyRelative("sesKlipleri").arraySize,
                Is.GreaterThan(0));
        }

        [Test]
        public void MapSceneReferencesLateEnemiesAndBothBossWaves()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Map.unity", OpenSceneMode.Single);
            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            Assert.That(spawner, Is.Not.Null);

            SerializedObject serialized = new(spawner);
            Assert.That(serialized.FindProperty("geçDönemDüşmanPrefabları").arraySize, Is.GreaterThanOrEqualTo(9));
            Assert.That(serialized.FindProperty("geçDönemBaşlangıçWave").intValue, Is.EqualTo(8));
            Assert.That(serialized.FindProperty("birinciBossPrefabı").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("birinciBossWave").intValue, Is.EqualTo(7));
            Assert.That(serialized.FindProperty("ikinciBossPrefabı").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("ikinciBossWave").intValue, Is.EqualTo(12));
            Assert.That(
                serialized.FindProperty("ikinciBossCanÇarpanı").floatValue,
                Is.GreaterThan(serialized.FindProperty("birinciBossCanÇarpanı").floatValue));
            Assert.That(serialized.FindProperty("birinciBossHasarÇarpanı").floatValue, Is.GreaterThanOrEqualTo(1f));
            Assert.That(serialized.FindProperty("ikinciBossHasarÇarpanı").floatValue, Is.GreaterThanOrEqualTo(1f));
        }
    }
}
