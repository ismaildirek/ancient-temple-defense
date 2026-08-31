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
    public sealed class BossAllyPresentationPlayModeTests
    {
        [UnityTest]
        public IEnumerator BossTwoFacesPlayerOnBothSides()
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
            yield return null;

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            player.enabled = false;
            Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
            playerBody.linearVelocity = Vector2.zero;
            playerBody.constraints = RigidbodyConstraints2D.FreezeAll;

            EnemyCombatant boss = Object.Instantiate(
                spawner.BossPrefabForWave(12),
                player.transform.position + Vector3.right * 6f,
                Quaternion.identity);
            BossEnemyBrain brain = boss.GetComponent<BossEnemyBrain>();
            SpriteRenderer sprite = boss.GetComponent<SpriteRenderer>();
            brain.Initialize(player.transform, 10, 1f);

            yield return new WaitForFixedUpdate();
            Assert.That(sprite.flipX, Is.True, "Boss 2 oyuncu solundayken sola dönmeli.");

            player.transform.position = boss.transform.position + Vector3.right * 6f;
            yield return new WaitForFixedUpdate();
            Assert.That(sprite.flipX, Is.False, "Boss 2 oyuncu sağındayken sağa dönmeli.");

            Object.Destroy(boss.gameObject);
        }

        [UnityTest]
        public IEnumerator PurchasedAlliesAlignTheirColliderFeetWithPlayerGround()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            spawner.enabled = false;
            foreach (FriendlyDefender defender in Object.FindObjectsByType<FriendlyDefender>(FindObjectsSortMode.None))
            {
                Object.Destroy(defender.gameObject);
            }
            yield return null;

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            DefenseShopPanel shop = Object.FindFirstObjectByType<DefenseShopPanel>();
            PlayerWallet wallet = player.GetComponent<PlayerWallet>();
            player.enabled = false;
            wallet.CoinEkle(100);

            shop.ShowShop(5);
            Assert.That(shop.TryBuy(0), Is.True, "Martial Hero satın alınabilmeli.");
            Assert.That(shop.TryBuy(1), Is.True, "Hero Knight satın alınabilmeli.");
            Physics2D.SyncTransforms();

            float playerGroundY = player.GetComponent<Collider2D>().bounds.min.y;
            FriendlyDefender[] allies = Object.FindObjectsByType<FriendlyDefender>(FindObjectsSortMode.None);
            Assert.That(allies.Length, Is.EqualTo(2));
            foreach (FriendlyDefender ally in allies)
            {
                float allyGroundY = ally.GetComponent<Collider2D>().bounds.min.y;
                Assert.That(allyGroundY, Is.EqualTo(playerGroundY).Within(0.01f), ally.name);
            }

            shop.CloseShop();
            foreach (FriendlyDefender ally in allies)
            {
                Object.Destroy(ally.gameObject);
            }
        }
        [UnityTest]
        public IEnumerator BothBossesTargetAndDamageTheNearestPurchasedAlly()
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
            foreach (FriendlyDefender defender in Object.FindObjectsByType<FriendlyDefender>(FindObjectsSortMode.None))
            {
                Object.Destroy(defender.gameObject);
            }
            yield return null;

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            DefenseShopPanel shop = Object.FindFirstObjectByType<DefenseShopPanel>();
            PlayerWallet wallet = player.GetComponent<PlayerWallet>();
            player.enabled = false;
            player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            wallet.CoinEkle(100);

            shop.ShowShop(5);
            Assert.That(shop.TryBuy(0), Is.True);
            shop.CloseShop();
            FriendlyDefender ally = Object.FindFirstObjectByType<FriendlyDefender>();
            Assert.That(ally, Is.Not.Null);
            ally.transform.position = Vector3.zero;
            ally.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            player.transform.position = Vector3.right * 12f;

            foreach (int bossWave in new[] { 7, 12 })
            {
                EnemyCombatant boss = Object.Instantiate(
                    spawner.BossPrefabForWave(bossWave),
                    ally.transform.position + Vector3.right * 0.8f,
                    Quaternion.identity);
                BossEnemyBrain brain = boss.GetComponent<BossEnemyBrain>();
                brain.Initialize(player.transform, 20, 5f);
                int healthBefore = ally.CurrentHealth;

                float deadline = Time.realtimeSinceStartup + 3f;
                while (ally.CurrentHealth == healthBefore && Time.realtimeSinceStartup < deadline)
                {
                    yield return null;
                }

                var targetField = typeof(BossEnemyBrain).GetField("_target", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                Assert.That(targetField?.GetValue(brain), Is.EqualTo(ally.transform), $"Wave {bossWave} bossu en yakın dostu hedeflemeli.");
                Assert.That(ally.CurrentHealth, Is.LessThan(healthBefore), $"Wave {bossWave} bossu dosta hasar vermeli.");
                Object.Destroy(boss.gameObject);
                yield return null;
            }

            Object.Destroy(ally.gameObject);
        }
    }
}
