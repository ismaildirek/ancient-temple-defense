using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class EnemyGroundAlignmentPlayModeTests
    {
        [UnityTest]
        public IEnumerator EveryGroundEnemySpawnsOnPlayerGroundContactLine()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            spawner.enabled = false;
            foreach (EnemyCombatant existing in Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None))
            {
                Object.Destroy(existing.gameObject);
            }

            yield return null;
            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            Assert.That(playerCollider, Is.Not.Null);
            float expectedGroundY = playerCollider.bounds.min.y;

            MethodInfo spawnMethod = typeof(EnemyWaveSpawner).GetMethod(
                "SpawnPrefab",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(spawnMethod, Is.Not.Null);

            List<EnemyCombatant> prefabs = new();
            prefabs.AddRange(GetPrefabArray(spawner, "düşmanPrefabları"));
            prefabs.AddRange(GetPrefabArray(spawner, "geçDönemDüşmanPrefabları"));
            prefabs.Add(spawner.BossPrefabForWave(7));
            prefabs.Add(spawner.BossPrefabForWave(12));

            List<EnemyCombatant> spawned = new();
            List<string> alignmentFailures = new();
            foreach (EnemyCombatant prefab in prefabs)
            {
                if (prefab == null || prefab.name.Contains("Flying", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                spawnMethod.Invoke(spawner, new object[] { prefab, 20, 1, 1f });
                EnemyCombatant enemy = spawner.transform.GetChild(spawner.transform.childCount - 1)
                    .GetComponent<EnemyCombatant>();
                spawned.Add(enemy);
                Physics2D.SyncTransforms();
                float actualGroundY = enemy.GetComponent<Collider2D>().bounds.min.y;
                if (Mathf.Abs(actualGroundY - expectedGroundY) > 0.06f)
                {
                    alignmentFailures.Add(
                        $"{prefab.name}: beklenen={expectedGroundY:F3}, gerçek={actualGroundY:F3}, fark={actualGroundY - expectedGroundY:F3}, rootY={enemy.transform.position.y:F3}");
                }
            }

            Assert.That(alignmentFailures, Is.Empty, string.Join("\n", alignmentFailures));

            foreach (EnemyCombatant enemy in spawned)
            {
                Object.Destroy(enemy.gameObject);
            }
        }

        private static EnemyCombatant[] GetPrefabArray(EnemyWaveSpawner spawner, string fieldName)
        {
            FieldInfo field = typeof(EnemyWaveSpawner).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (EnemyCombatant[])field.GetValue(spawner);
        }
    }
}
