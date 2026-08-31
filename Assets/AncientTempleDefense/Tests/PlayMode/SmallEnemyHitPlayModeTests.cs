using System.Collections;
using System.Linq;
using System.Reflection;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class SmallEnemyHitPlayModeTests
    {
        [UnityTest]
        public IEnumerator RatAndSlimeCanBeHitAtEdgeOfVisibleSwordSweep()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            Assert.That(spawner, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            spawner.enabled = false;
            player.enabled = false;
            ClearEnemies();
            yield return null;

            FieldInfo facingField = typeof(BlackKnightPlayerController).GetField("_facing", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo dealDamage = typeof(BlackKnightPlayerController).GetMethod("DealDamage", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo insideSweep = typeof(BlackKnightPlayerController).GetMethod("IsInsideDirectedAttackSweep", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(facingField, Is.Not.Null);
            Assert.That(dealDamage, Is.Not.Null);
            Assert.That(insideSweep, Is.Not.Null);
            facingField.SetValue(player, 1);

            foreach (string prefabName in new[] { "RatRaiderEnemy", "ExplodingSlimeEnemy" })
            {
                EnemyCombatant prefab = spawner.LateEnemyPrefabs.FirstOrDefault(item => item != null && item.name == prefabName);
                Assert.That(prefab, Is.Not.Null, prefabName);
                EnemyCombatant enemy = Object.Instantiate(prefab, player.transform.position + Vector3.right * 2.05f, Quaternion.identity);
                enemy.GetComponent<EnemyBrain>().enabled = false;
                enemy.ConfigureForWave(3);
                Collider2D collider = enemy.GetComponent<Collider2D>();
                enemy.transform.position += Vector3.up * (player.transform.position.y - collider.bounds.center.y);
                Physics2D.SyncTransforms();

                Assert.That(EnemyCombatant.ActiveEnemies.Contains(enemy), Is.True, prefabName + " aktif düşman kaydında yok.");
                bool inside = (bool)insideSweep.Invoke(player, new object[] { enemy, 1f });
                Assert.That(inside, Is.True, prefabName + " yönlü kılıç taramasının dışında kaldı. Merkez=" + collider.bounds.center + " Oyuncu=" + player.transform.position + " Menzil=" + player.AttackReach + " Pay=" + player.SmallEnemyAssistMargin);

                int healthBefore = enemy.RemainingHits;
                dealDamage.Invoke(player, new object[] { 1f, 1 });
                Assert.That(enemy.RemainingHits, Is.EqualTo(healthBefore - 1), prefabName + " kılıç taramasında vurulamadı.");
                Object.Destroy(enemy.gameObject);
                yield return null;
            }
        }

        private static void ClearEnemies()
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