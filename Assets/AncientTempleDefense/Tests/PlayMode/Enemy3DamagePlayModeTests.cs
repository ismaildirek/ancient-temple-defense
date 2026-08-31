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
    public sealed class Enemy3DamagePlayModeTests
    {
        [UnityTest]
        public IEnumerator Enemy3ReceivesDamageFromPlayerAttackArea()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            spawner.enabled = false;
            foreach (EnemyCombatant activeEnemy in Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None))
            {
                Object.Destroy(activeEnemy.gameObject);
            }

            yield return null;

            FieldInfo latePrefabsField = typeof(EnemyWaveSpawner).GetField(
                "geçDönemDüşmanPrefabları",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(latePrefabsField, Is.Not.Null);
            EnemyCombatant[] latePrefabs = (EnemyCombatant[])latePrefabsField.GetValue(spawner);
            EnemyCombatant enemyPrefab = latePrefabs.Single(
                prefab => prefab != null && prefab.name == "NewEnemy3");

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            player.enabled = false;
            EnemyCombatant enemy = Object.Instantiate(
                enemyPrefab,
                player.transform.position + Vector3.right * 0.7f,
                Quaternion.identity);
            enemy.GetComponent<EnemyBrain>().enabled = false;
            enemy.ConfigureForWave(3);

            Collider2D playerCollider = player.GetComponent<Collider2D>();
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            Physics2D.SyncTransforms();
            enemy.transform.position += Vector3.up * (playerCollider.bounds.min.y - enemyCollider.bounds.min.y);
            Physics2D.SyncTransforms();

            int healthBefore = enemy.RemainingHits;
            MethodInfo dealDamage = typeof(BlackKnightPlayerController).GetMethod(
                "DealDamage",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(dealDamage, Is.Not.Null);
            dealDamage.Invoke(player, new object[] { 1f, 1 });

            Assert.That(enemy.RemainingHits, Is.EqualTo(healthBefore - 1));
            Object.Destroy(enemy.gameObject);
        }
    }
}
