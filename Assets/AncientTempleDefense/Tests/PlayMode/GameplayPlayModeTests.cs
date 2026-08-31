using System.Collections;
using System.Linq;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class GameplayPlayModeTests
    {
        [UnityTest]
        public IEnumerator MapStartsSpawnsEveryEnemyTypeAndThirdHitDespawnsEnemy()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            Assert.That(player, Is.Not.Null);
            Assert.That(spawner, Is.Not.Null);

            float spawnDeadline = Time.realtimeSinceStartup + 6f;
            while (spawner.AliveEnemies < spawner.CurrentWaveTotal
                   && Time.realtimeSinceStartup < spawnDeadline)
            {
                yield return null;
            }

            EnemyCombatant[] enemies = Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None);
            Assert.That(enemies.Length, Is.GreaterThanOrEqualTo(spawner.CurrentWaveTotal));

            string[] spawnedNames = enemies.Select(enemy => enemy.name).ToArray();
            Assert.That(spawnedNames.Any(name => name.StartsWith("SkeletonEnemy")), Is.True);
            Assert.That(spawnedNames.Any(name => name.StartsWith("GoblinEnemy")), Is.True);
            Assert.That(spawnedNames.Any(name => name.StartsWith("MushroomEnemy")), Is.True);
            Assert.That(spawnedNames.Any(name => name.StartsWith("FlyingEyeEnemy")), Is.True);

            EnemyCombatant heavyTarget = enemies[0];
            GameObject heavyTargetObject = heavyTarget.gameObject;
            Assert.That(heavyTarget.MaximumHits, Is.GreaterThanOrEqualTo(2));
            heavyTarget.TakeHit(heavyTarget.MaximumHits - 1);
            Assert.That(heavyTarget.RemainingHits, Is.EqualTo(1));
            heavyTarget.TakeHit(1);
            Assert.That(heavyTarget.IsDead, Is.True);

            EnemyCombatant ultimateTarget = enemies[1];
            GameObject ultimateTargetObject = ultimateTarget.gameObject;
            ultimateTarget.TakeHit(ultimateTarget.MaximumHits);
            Assert.That(ultimateTarget.IsDead, Is.True);

            yield return new WaitForSeconds(2.8f);
            Assert.That(heavyTargetObject == null, Is.True);
            Assert.That(ultimateTargetObject == null, Is.True);
        }
    }
}
