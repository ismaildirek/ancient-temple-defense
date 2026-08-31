using System.Collections;
using AncientTempleDefense.Enemies;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class LateGameEnemyPlayModeTests
    {
        [UnityTest]
        public IEnumerator SceneRosterUnlocksLateEnemiesAndReferencesIncreasingBossTiers()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            Assert.That(spawner, Is.Not.Null);
            Assert.That(spawner.LateEnemiesUnlocked(7), Is.False);
            Assert.That(spawner.LateEnemiesUnlocked(8), Is.True);

            EnemyCombatant bossOne = spawner.BossPrefabForWave(7);
            EnemyCombatant bossTwo = spawner.BossPrefabForWave(12);
            Assert.That(bossOne, Is.Not.Null);
            Assert.That(bossTwo, Is.Not.Null);
            Assert.That(spawner.BossPrefabForWave(8), Is.Null);
            Assert.That(bossOne.GetComponent<BossEnemyBrain>().BossTier, Is.EqualTo(1));
            Assert.That(bossTwo.GetComponent<BossEnemyBrain>().BossTier, Is.EqualTo(2));
            Assert.That(
                bossTwo.GetComponent<BossEnemyBrain>().SpecialDamageMultiplier,
                Is.GreaterThan(bossOne.GetComponent<BossEnemyBrain>().SpecialDamageMultiplier));
        }
    }
}
