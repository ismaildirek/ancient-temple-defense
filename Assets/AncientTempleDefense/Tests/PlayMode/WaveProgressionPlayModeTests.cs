using System.Collections;
using System.Linq;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.Progression;
using AncientTempleDefense.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class WaveProgressionPlayModeTests
    {
        [TearDown]
        public void RestoreTimeScale()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator FirstWaveUsesBaseStatsAndThaleahHud()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            float firstWaveDeadline = Time.realtimeSinceStartup + 6f;
            while (spawner.AliveEnemies < spawner.CurrentWaveTotal
                   && Time.realtimeSinceStartup < firstWaveDeadline)
            {
                yield return null;
            }
            WaveUpgradePanel panel = Object.FindFirstObjectByType<WaveUpgradePanel>();
            PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
            EnemyCombatant[] living = Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None)
                .Where(enemy => !enemy.IsDead)
                .ToArray();

            Assert.That(spawner, Is.Not.Null);
            Assert.That(panel, Is.Not.Null);
            Assert.That(playerHealth, Is.Not.Null);
            Assert.That(spawner.CurrentWave, Is.EqualTo(1));
            Assert.That(spawner.CurrentWaveTotal, Is.GreaterThan(0));
            Assert.That(spawner.AliveEnemies, Is.EqualTo(spawner.CurrentWaveTotal));
            Assert.That(living, Has.Length.EqualTo(spawner.CurrentWaveTotal));
            Assert.That(living.All(enemy => enemy.MaximumHits == spawner.CurrentEnemyHealth), Is.True);
            Assert.That(living.All(enemy => enemy.GetComponent<EnemyBrain>().AttackDamage == spawner.CurrentEnemyDamage), Is.True);
            Assert.That(living.All(enemy => Mathf.Approximately(
                enemy.GetComponent<EnemyBrain>().AttackSpeedMultiplier, spawner.CurrentAttackSpeedMultiplier)), Is.True);
            Assert.That(panel.PixelFont, Is.Not.Null);
            Assert.That(panel.PixelFont.name, Does.Contain("Thaleah"));
            Assert.That(GameObject.Find("WaveText"), Is.Not.Null);
            Assert.That(GameObject.Find("EnemyText"), Is.Null, "Wave HUD yalnızca wave numarasını göstermeli.");
            Assert.That(GameObject.Find("StatsText"), Is.Null, "HUD hız ve saldırı istatistiklerini göstermemeli.");
            Assert.That(GameObject.Find("HealthText").GetComponent<UnityEngine.UI.Text>().fontSize, Is.EqualTo(42));
        }

        [UnityTest]
public IEnumerator UpgradeCardPausesGameAndChangesSelectedPlayerStat()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            WaveUpgradePanel panel = Object.FindFirstObjectByType<WaveUpgradePanel>();
            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            int lightBefore = player.LightAttackDamage;
            int heavyBefore = player.HeavyAttackDamage;
            int ultimateBefore = player.UltimateAttackDamage;
            float speedBefore = player.MoveSpeed;
            float reachBefore = player.AttackReach;
            float cooldownBefore = player.UltimateCooldown;
            int maximumHealthBefore = health.MaximumHealth;
            int armorBefore = health.Armor;

            panel.ShowChoices(5);
            yield return null;

            Assert.That(panel.IsChoosing, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            Assert.That(panel.VisibleChoices, Has.Count.EqualTo(3));
            Assert.That(panel.VisibleChoices.Select(card => card.Type).Distinct().Count(), Is.EqualTo(3));

            PlayerUpgradeType selected = panel.VisibleChoices[0].Type;
            panel.SelectChoice(0);
            Assert.That(panel.IsChoosing, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));

            switch (selected)
            {
                case PlayerUpgradeType.LightDamage:
                    Assert.That(player.LightAttackDamage, Is.EqualTo(lightBefore + 1));
                    break;
                case PlayerUpgradeType.HeavyDamage:
                    Assert.That(player.HeavyAttackDamage, Is.EqualTo(heavyBefore + 1));
                    break;
                case PlayerUpgradeType.UltimateDamage:
                    Assert.That(player.UltimateAttackDamage, Is.EqualTo(ultimateBefore + 2));
                    break;
                case PlayerUpgradeType.MoveSpeed:
                    Assert.That(player.MoveSpeed, Is.GreaterThan(speedBefore));
                    break;
                case PlayerUpgradeType.AttackReach:
                    Assert.That(player.AttackReach, Is.GreaterThan(reachBefore));
                    break;
                case PlayerUpgradeType.UltimateCooldown:
                    Assert.That(player.UltimateCooldown, Is.LessThan(cooldownBefore));
                    break;
                case PlayerUpgradeType.MaximumHealth:
                    Assert.That(health.MaximumHealth, Is.EqualTo(maximumHealthBefore + 25));
                    break;
                case PlayerUpgradeType.Armor:
                    Assert.That(health.Armor, Is.EqualTo(armorBefore + 8));
                    break;
            }
        }

        [UnityTest]
        public IEnumerator ClearingWaveStartsStrongerSecondWave()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            float spawnDeadline = Time.realtimeSinceStartup + 6f;
            while (spawner.AliveEnemies < spawner.CurrentWaveTotal
                   && Time.realtimeSinceStartup < spawnDeadline)
            {
                yield return null;
            }

            int firstWaveTotal = spawner.CurrentWaveTotal;
            foreach (EnemyCombatant enemy in Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None))
            {
                enemy.TakeHit(999);
            }

            float secondWaveDeadline = Time.realtimeSinceStartup + 10f;
            while ((spawner.CurrentWave < 2 || spawner.AliveEnemies < 1)
                   && Time.realtimeSinceStartup < secondWaveDeadline)
            {
                yield return null;
            }

            EnemyCombatant[] living = Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None)
                .Where(enemy => !enemy.IsDead)
                .ToArray();
            Assert.That(spawner.CurrentWave, Is.EqualTo(2));
            Assert.That(spawner.CurrentWaveTotal, Is.GreaterThan(firstWaveTotal));
            Assert.That(spawner.CurrentEnemyHealth, Is.GreaterThanOrEqualTo(1));
            Assert.That(spawner.CurrentEnemyDamage, Is.GreaterThanOrEqualTo(1));
            Assert.That(spawner.CurrentAttackSpeedMultiplier, Is.GreaterThan(1f));
            Assert.That(living.Length, Is.GreaterThanOrEqualTo(1));
            Assert.That(living.All(enemy => enemy.MaximumHits == spawner.CurrentEnemyHealth), Is.True);
            Assert.That(living.All(enemy => enemy.GetComponent<EnemyBrain>().AttackDamage == spawner.CurrentEnemyDamage), Is.True);
        }
    }
}
