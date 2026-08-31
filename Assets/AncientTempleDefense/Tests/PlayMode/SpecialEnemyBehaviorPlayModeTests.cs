using System.Collections;
using System.Linq;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.Temple;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class SpecialEnemyBehaviorPlayModeTests
    {
        [UnityTest]
        public IEnumerator WolfUnlocksAtWaveNineAndTargetsOnlyPlayer()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            Assert.That(spawner, Is.Not.Null);
            spawner.enabled = false;
            ClearSpawnedEnemies();
            yield return null;

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            TempleHealth temple = Object.FindFirstObjectByType<TempleHealth>();
            Assert.That(player, Is.Not.Null);
            Assert.That(temple, Is.Not.Null);
            Assert.That(spawner.WolfUnlocked(8), Is.False);
            Assert.That(spawner.WolfUnlocked(9), Is.True);

            player.transform.position = temple.transform.position + Vector3.right * 8f;
            EnemyCombatant wolf = Object.Instantiate(
                spawner.WolfPrefab,
                temple.transform.position,
                Quaternion.identity);
            wolf.ConfigureForWave(10);
            EnemyBrain brain = wolf.GetComponent<EnemyBrain>();
            brain.Initialize(null, 12, 1f);

            yield return new WaitForSeconds(0.35f);

            Assert.That(brain.TargetMode, Is.EqualTo(EnemyTargetMode.PlayerOnly));
            Assert.That(object.ReferenceEquals(brain.CurrentTarget, player.transform), Is.True);
            Assert.That(object.ReferenceEquals(brain.CurrentTarget, temple.transform), Is.False);
            Assert.That(
                wolf.GetComponent<SpriteRenderer>().flipX,
                Is.True,
                "Kaynak Wolf sprite\u0027ı sola baktığı için sağdaki oyuncuya dönerken çevrilmeli.");
            Object.Destroy(wolf.gameObject);
        }

        [UnityTest]
        public IEnumerator ExplodingSlimeDamagesNearbyPlayerOnceOnDeath()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            Assert.That(spawner, Is.Not.Null);
            spawner.enabled = false;
            ClearSpawnedEnemies();
            yield return null;

            PlayerHealth player = Object.FindFirstObjectByType<PlayerHealth>();
            Assert.That(player, Is.Not.Null);
            EnemyCombatant slimePrefab = spawner.LateEnemyPrefabs
                .FirstOrDefault(enemy => enemy != null && enemy.name == "ExplodingSlimeEnemy");
            Assert.That(slimePrefab, Is.Not.Null);

            EnemyCombatant slime = Object.Instantiate(
                slimePrefab,
                player.transform.position,
                Quaternion.identity);
            slime.ConfigureForWave(1);
            EnemyBrain brain = slime.GetComponent<EnemyBrain>();
            brain.Initialize(player.transform, 20, 1f);
            brain.enabled = false;
            Physics2D.SyncTransforms();

            int healthBefore = player.CurrentHealth;
            slime.TakeHit(1);
            yield return null;

            ExplodingEnemy explosion = slime.GetComponent<ExplodingEnemy>();
            Assert.That(explosion.ExplosionCount, Is.EqualTo(1));
            Assert.That(explosion.LastExplosionDamage, Is.GreaterThan(0));
            Assert.That(player.CurrentHealth, Is.LessThan(healthBefore));

            GameObject runtimeEffect = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Select(item => item.gameObject)
                .FirstOrDefault(item => item.name == "ExplosionEffect4_Runtime");
            Assert.That(runtimeEffect, Is.Not.Null);
            foreach (ParticleSystem particle in runtimeEffect.GetComponentsInChildren<ParticleSystem>(true))
            {
                Assert.That(particle.main.scalingMode, Is.EqualTo(ParticleSystemScalingMode.Hierarchy));
            }
        }

        private static void ClearSpawnedEnemies()
        {
            foreach (EnemyCombatant enemy in EnemyCombatant.ActiveEnemies.ToArray())
            {
                if (enemy != null)
                {
                    Object.Destroy(enemy.gameObject);
                }
            }
        }
    }
}
