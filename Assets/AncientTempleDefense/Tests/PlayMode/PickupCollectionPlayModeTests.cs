using System.Collections;
using System.Reflection;
using AncientTempleDefense.Economy;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AncientTempleDefense.Tests
{
    public sealed class PickupCollectionPlayModeTests
    {
        [UnityTest]
        public IEnumerator DelayedOverlappingPickupsCollectCoinAndHealFifty()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            if (spawner != null)
            {
                spawner.enabled = false;
            }

            foreach (EnemyCombatant enemy in Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None))
            {
                Object.Destroy(enemy.gameObject);
            }

            yield return null;

            PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
            PlayerWallet wallet = health != null ? health.GetComponent<PlayerWallet>() : null;
            Assert.That(health, Is.Not.Null);
            Assert.That(wallet, Is.Not.Null);

            int initialCoin = wallet.Coin;
            GameObject coin = CreatePickup(PickupType.Coin, 1, health.transform.position);
            yield return new WaitForSeconds(0.2f);
            InvokeTriggerStay(coin.GetComponent<WorldPickup>(), health.GetComponent<Collider2D>());
            yield return null;

            Assert.That(wallet.Coin, Is.EqualTo(initialCoin + 1));
            Assert.That(coin == null, Is.True);

            health.TakeDamage(60);
            int healthBeforePotion = health.CurrentHealth;
            GameObject potion = CreatePickup(PickupType.HealthPotion, 50, health.transform.position);
            yield return new WaitForSeconds(0.2f);
            InvokeTriggerStay(potion.GetComponent<WorldPickup>(), health.GetComponent<Collider2D>());
            yield return null;

            Assert.That(health.CurrentHealth, Is.EqualTo(Mathf.Min(health.MaximumHealth, healthBeforePotion + 50)));
            Assert.That(potion == null, Is.True);
        }

        private static void InvokeTriggerStay(WorldPickup pickup, Collider2D playerCollider)
        {
            MethodInfo method = typeof(WorldPickup).GetMethod(
                "OnTriggerStay2D",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            Assert.That(playerCollider, Is.Not.Null);
            method.Invoke(pickup, new object[] { playerCollider });
        }

        private static GameObject CreatePickup(PickupType type, int value, Vector3 position)
        {
            GameObject go = new("TestPickup");
            go.SetActive(false);
            go.transform.position = position;
            go.AddComponent<SpriteRenderer>();
            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            WorldPickup pickup = go.AddComponent<WorldPickup>();
            SetField(pickup, "esyaTuru", type);
            SetField(pickup, "deger", value);
            SetField(pickup, "toplanmaGecikmesi", 0.12f);
            go.SetActive(true);
            Physics2D.SyncTransforms();
            return go;
        }

        private static void SetField<T>(WorldPickup pickup, string fieldName, T value)
        {
            FieldInfo field = typeof(WorldPickup).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(pickup, value);
        }
    }
}
