using System.Collections;
using AncientTempleDefense.Allies;
using AncientTempleDefense.Economy;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class RosterExpansionPlayModeTests
    {
        [UnityTest]
        public IEnumerator CheapSoldierCostsTenAndAlignsWithPlayerGround()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;
            Object.FindFirstObjectByType<EnemyWaveSpawner>().enabled = false;
            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            player.enabled = false;
            PlayerWallet wallet = player.GetComponent<PlayerWallet>();
            wallet.CoinEkle(10);
            DefenseShopPanel shop = Object.FindFirstObjectByType<DefenseShopPanel>();
            shop.ShowShop(5);
            Assert.That(shop.TryBuy(2), Is.True);
            Assert.That(wallet.Coin, Is.EqualTo(0));
            Physics2D.SyncTransforms();
            FriendlyDefender soldier = Object.FindFirstObjectByType<FriendlyDefender>();
            Assert.That(soldier, Is.Not.Null);
            Assert.That(soldier.GetComponent<Collider2D>().bounds.min.y,
                Is.EqualTo(player.GetComponent<Collider2D>().bounds.min.y).Within(.01f));
            shop.CloseShop();
            Object.Destroy(soldier.gameObject);
        }

        [UnityTest]
        public IEnumerator KillingBossFourShowsVictoryScore()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;
            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            spawner.enabled = false;
            foreach (EnemyCombatant active in Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None))
                Object.Destroy(active.gameObject);
            yield return null;

            GameOverPanel panel = Object.FindFirstObjectByType<GameOverPanel>();
            EnemyCombatant boss = Object.Instantiate(spawner.BossPrefabForWave(22), Vector3.zero, Quaternion.identity);
            boss.ConfigureForWave(1);
            boss.TakeHit(1);
            yield return null;
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(panel.IsVictory, Is.True);
            Assert.That(panel.KilledEnemies, Is.EqualTo(1));
            Time.timeScale = 1f;
            Object.Destroy(boss.gameObject);
        }
    }
}
