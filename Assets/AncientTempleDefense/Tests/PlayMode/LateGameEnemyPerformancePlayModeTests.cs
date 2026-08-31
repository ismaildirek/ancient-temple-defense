using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AncientTempleDefense.Tests
{
    public sealed class LateGameEnemyPerformancePlayModeTests
    {
        [UnityTest]
        public IEnumerator EightOffscreenLateEnemiesProduceAStableMainThreadSample()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            Assert.That(spawner, Is.Not.Null);
            spawner.enabled = false;

            foreach (EnemyCombatant existing in Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None))
            {
                Object.Destroy(existing.gameObject);
            }

            yield return null;
            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            Assert.That(player, Is.Not.Null);

            EnemyCombatant[] prefabs = LateEnemyPrefabs(spawner);
            Assert.That(prefabs.Length, Is.GreaterThanOrEqualTo(9));
            List<EnemyCombatant> spawned = new();
            for (int index = 0; index < 8; index++)
            {
                EnemyCombatant enemy = Object.Instantiate(
                    prefabs[index % prefabs.Length],
                    new Vector3(100f + index * 2f, -3.25f, 0f),
                    Quaternion.identity);
                enemy.GetComponent<EnemyBrain>().Initialize(player.transform, 5, 1f);
                spawned.Add(enemy);
            }

            for (int frame = 0; frame < 45; frame++)
            {
                yield return null;
            }

            const int measuredFrames = 180;
            using ProfilerRecorder mainThread = ProfilerRecorder.StartNew(
                ProfilerCategory.Internal,
                "Main Thread",
                measuredFrames);
            for (int frame = 0; frame < measuredFrames; frame++)
            {
                yield return null;
            }

            long[] samples = mainThread.ToArray()
                .Select(sample => sample.Value)
                .Where(value => value > 0)
                .OrderBy(value => value)
                .ToArray();
            Assert.That(mainThread.Valid, Is.True);
            Assert.That(samples.Length, Is.GreaterThan(30));

            long medianNanoseconds = samples[samples.Length / 2];
            long percentile95Nanoseconds = samples[Math.Min(samples.Length - 1, (int)(samples.Length * 0.95f))];
            Debug.Log(
                $"LATE_GAME_PERF median_ms={medianNanoseconds / 1_000_000f:F3} p95_ms={percentile95Nanoseconds / 1_000_000f:F3} samples={samples.Length}");

            foreach (EnemyCombatant enemy in spawned)
            {
                Object.Destroy(enemy.gameObject);
            }
        }

        private static EnemyCombatant[] LateEnemyPrefabs(EnemyWaveSpawner spawner)
        {
            FieldInfo field = typeof(EnemyWaveSpawner).GetField(
                "geçDönemDüşmanPrefabları",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (EnemyCombatant[])field.GetValue(spawner);
        }
    }
}
