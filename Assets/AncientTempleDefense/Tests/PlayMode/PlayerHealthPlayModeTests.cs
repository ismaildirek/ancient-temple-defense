using System.Collections;
using AncientTempleDefense.Player;
using AncientTempleDefense.Progression;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class PlayerHealthPlayModeTests
    {
        [UnityTest]
        public IEnumerator DamageArmorAndMaximumHealthUpgradesUseExpectedValues()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            int initialMaximumHealth = health.MaximumHealth;
            Assert.That(health.CurrentHealth, Is.EqualTo(initialMaximumHealth));

            health.TakeDamage(20);
            Assert.That(health.CurrentHealth, Is.EqualTo(initialMaximumHealth - 20));

            yield return new WaitForSecondsRealtime(0.4f);
            player.ApplyUpgrade(PlayerUpgradeType.Armor);
            Assert.That(health.Armor, Is.EqualTo(8));
            health.TakeDamage(20);
            Assert.That(health.CurrentHealth, Is.EqualTo(initialMaximumHealth - 39));

            player.ApplyUpgrade(PlayerUpgradeType.MaximumHealth);
            Assert.That(health.MaximumHealth, Is.EqualTo(initialMaximumHealth + 25));
            Assert.That(health.CurrentHealth, Is.EqualTo(initialMaximumHealth - 14));
        }

        [UnityTest]
        public IEnumerator LethalDamageDisablesPlayerController()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            health.TakeDamage(health.MaximumHealth + 999);

            Assert.That(health.IsDead, Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(0));
            Assert.That(player.enabled, Is.False);
        }
    }
}

