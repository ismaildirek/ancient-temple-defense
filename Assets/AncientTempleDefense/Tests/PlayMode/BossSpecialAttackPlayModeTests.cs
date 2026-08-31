using System.Collections;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AncientTempleDefense.Tests
{
    public sealed class BossSpecialAttackPlayModeTests
    {
        [UnityTest]
        public IEnumerator BothBossesExecuteSpecialAttacksAndBossTwoDealsMoreDamage()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            spawner.enabled = false;
            foreach (EnemyCombatant enemy in Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None))
            {
                Object.Destroy(enemy.gameObject);
            }

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            Text healthText = GameObject.Find("HealthText").GetComponent<Text>();
            int healthBeforeBossAttack = playerHealth.CurrentHealth;
            player.enabled = false;
            Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
            playerBody.linearVelocity = Vector2.zero;
            playerBody.constraints = RigidbodyConstraints2D.FreezeAll;
            player.transform.position = new Vector3(0f, -3.25f, 0f);
            yield return null;

            EnemyCombatant bossOne = Object.Instantiate(
                spawner.BossPrefabForWave(7),
                player.transform.position + Vector3.right * 0.8f,
                Quaternion.identity);
            BossEnemyBrain bossOneBrain = bossOne.GetComponent<BossEnemyBrain>();
            float bossOneStartX = bossOne.transform.position.x;
            float bossOneStartY = bossOne.transform.position.y;
            bossOneBrain.Initialize(player.transform, 10, 5f);

            float bossOneDeadline = Time.realtimeSinceStartup + 8f;
            while (bossOneBrain.SpecialAttackCount < 1
                   && Time.realtimeSinceStartup < bossOneDeadline)
            {
                yield return null;
            }

            Assert.That(bossOneBrain.LastAttackState, Is.EqualTo("Spell"));
            Assert.That(bossOneBrain.LastSpecialDamage, Is.EqualTo(24));
            Assert.That(bossOneBrain.TeleportCount, Is.EqualTo(1));
            Assert.That(Mathf.Abs(bossOne.GetComponent<Rigidbody2D>().position.x - bossOneStartX), Is.GreaterThan(5f));
            Assert.That(bossOne.GetComponent<Rigidbody2D>().position.y, Is.EqualTo(bossOneStartY).Within(0.001f));
            Assert.That(playerHealth.CurrentHealth, Is.LessThan(healthBeforeBossAttack));
            Assert.That(healthText.text, Is.EqualTo($"CAN {playerHealth.CurrentHealth}/{playerHealth.MaximumHealth}   ZIRH {playerHealth.Armor}"));
            int bossOneSpecialDamage = bossOneBrain.LastSpecialDamage;
            Object.Destroy(bossOne.gameObject);
            yield return null;

            EnemyCombatant bossTwo = Object.Instantiate(
                spawner.BossPrefabForWave(12),
                player.transform.position + Vector3.right * 0.8f,
                Quaternion.identity);
            BossEnemyBrain bossTwoBrain = bossTwo.GetComponent<BossEnemyBrain>();
            bossTwoBrain.Initialize(player.transform, 10, 5f);

            float bossTwoDeadline = Time.realtimeSinceStartup + 8f;
            while (bossTwoBrain.SpecialAttackCount < 1
                   && Time.realtimeSinceStartup < bossTwoDeadline)
            {
                yield return null;
            }

            Assert.That(bossTwoBrain.LastAttackState, Is.EqualTo("Special2"));
            Assert.That(bossTwoBrain.LastSpecialDamage, Is.EqualTo(30));
            Assert.That(bossTwoBrain.LastSpecialDamage, Is.GreaterThan(bossOneSpecialDamage));
            Assert.That(bossTwoBrain.TeleportCount, Is.Zero);
            Object.Destroy(bossTwo.gameObject);
        }
        [UnityTest]
        public IEnumerator BothBossesEnterSecondPhaseOnceAtHalfHealth()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            spawner.enabled = false;
            foreach (EnemyCombatant enemy in Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None))
            {
                Object.Destroy(enemy.gameObject);
            }

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            player.enabled = false;
            yield return null;

            foreach (int bossWave in new[] { 7, 12 })
            {
                EnemyCombatant boss = Object.Instantiate(
                    spawner.BossPrefabForWave(bossWave),
                    player.transform.position + Vector3.right * 8f,
                    Quaternion.identity);
                BossEnemyBrain brain = boss.GetComponent<BossEnemyBrain>();
                boss.ConfigureForWave(20);
                brain.Initialize(player.transform, 10, 1f);
                Vector3 firstPhaseScale = boss.transform.localScale;

                boss.TakeHit(9);
                Assert.That(brain.IsPhaseTwo, Is.False, $"Wave {bossWave} bossu yüzde 50 üstünde ikinci faza geçmemeli.");
                boss.TakeHit(1);

                Assert.That(brain.IsPhaseTwo, Is.True, $"Wave {bossWave} bossu yüzde 50 canda ikinci faza geçmeli.");
                Assert.That(brain.PhaseTransitionCount, Is.EqualTo(1));
                Assert.That(brain.EffectiveAttackSpeedMultiplier, Is.GreaterThan(brain.AttackSpeedMultiplier));
                Assert.That(brain.EffectiveDamageMultiplier, Is.GreaterThan(1f));
                Assert.That(boss.transform.localScale.x, Is.GreaterThan(firstPhaseScale.x));

                yield return new WaitForSeconds(0.15f);
                Assert.That(boss.GetComponent<SpriteRenderer>().color, Is.Not.EqualTo(Color.white));
                boss.TakeHit(1);
                Assert.That(brain.PhaseTransitionCount, Is.EqualTo(1), "İkinci faz yalnızca bir kez tetiklenmeli.");

                Object.Destroy(boss.gameObject);
                yield return null;
            }
        }
    }
}