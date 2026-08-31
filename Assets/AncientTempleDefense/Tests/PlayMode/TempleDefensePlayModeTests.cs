using System.Collections;
using System.Reflection;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.Temple;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AncientTempleDefense.Tests
{
    public sealed class TempleDefensePlayModeTests
    {
        [UnityTest]
        public IEnumerator TempleHudAndThreeDamageAppearancesFollowAuthoritativeHealth()
        {
            Time.timeScale = 1f;
            yield return LoadQuietMap();

            TempleHealth temple = Object.FindFirstObjectByType<TempleHealth>();
            Text templeText = GameObject.Find("TempleHealthText").GetComponent<Text>();
            Image templeFill = GameObject.Find("TempleHealthBarFill").GetComponent<Image>();
            SpriteRenderer templeRenderer = temple.GetComponent<SpriteRenderer>();
            Color healthyColor = templeRenderer.color;

            Assert.That(temple.MaximumHealth, Is.EqualTo(2000));
            Assert.That(temple.CurrentHealth, Is.EqualTo(2000));
            Assert.That(temple.DamageStage, Is.EqualTo(TempleDamageStage.Sağlam));
            Assert.That(templeText.text, Is.EqualTo("TAPINAK 2000/2000"));
            Assert.That(templeFill.fillAmount, Is.EqualTo(1f).Within(0.001f));

            temple.TakeDamage(700);
            yield return new WaitForSeconds(0.15f);
            Color damagedColor = templeRenderer.color;
            Assert.That(temple.DamageStage, Is.EqualTo(TempleDamageStage.Hasarlı));
            Assert.That(templeText.text, Is.EqualTo("TAPINAK 1300/2000"));
            Assert.That(templeFill.fillAmount, Is.EqualTo(0.65f).Within(0.001f));
            Assert.That(damagedColor, Is.Not.EqualTo(healthyColor));

            temple.TakeDamage(700);
            yield return new WaitForSeconds(0.15f);
            Assert.That(temple.DamageStage, Is.EqualTo(TempleDamageStage.Kritik));
            Assert.That(templeText.text, Is.EqualTo("TAPINAK 600/2000"));
            Assert.That(templeFill.fillAmount, Is.EqualTo(0.30f).Within(0.001f));
            Assert.That(templeRenderer.color, Is.Not.EqualTo(damagedColor));

            bool destroyed = false;
            temple.Destroyed += () => destroyed = true;
            temple.TakeDamage(600);
            Assert.That(temple.IsDestroyed, Is.True);
            Assert.That(destroyed, Is.True);
            Assert.That(templeText.text, Is.EqualTo("TAPINAK 0/2000"));
            Assert.That(templeFill.fillAmount, Is.Zero.Within(0.001f));
        }

        [UnityTest]
        public IEnumerator GroundEnemySelectsAndDamagesTempleWhenItIsClosest()
        {
            Time.timeScale = 1f;
            yield return LoadQuietMap();

            TempleHealth temple = Object.FindFirstObjectByType<TempleHealth>();
            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            player.enabled = false;
            player.transform.position = new Vector3(25f, player.transform.position.y, 0f);

            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            FieldInfo prefabField = typeof(EnemyWaveSpawner).GetField(
                "düşmanPrefabları",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(prefabField, Is.Not.Null);
            EnemyCombatant[] prefabs = (EnemyCombatant[])prefabField.GetValue(spawner);
            EnemyCombatant prefab = prefabs[0];
            EnemyCombatant attacker = Object.Instantiate(
                prefab,
                temple.transform.position + Vector3.right * 0.7f,
                Quaternion.identity);
            EnemyBrain brain = attacker.GetComponent<EnemyBrain>();
            Assert.That(brain, Is.Not.Null, "Test için normal düşman prefabı gerekli.");
            brain.Initialize(player.transform, 25, 5f);

            int initialHealth = temple.CurrentHealth;
            float deadline = Time.realtimeSinceStartup + 3f;
            while (temple.CurrentHealth == initialHealth && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(temple.CurrentHealth, Is.LessThan(initialHealth));
            Object.Destroy(attacker.gameObject);
        }

        private static IEnumerator LoadQuietMap()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;
            EnemyWaveSpawner spawner = Object.FindFirstObjectByType<EnemyWaveSpawner>();
            spawner.enabled = false;
            foreach (EnemyCombatant enemy in Object.FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None))
            {
                Object.Destroy(enemy.gameObject);
            }

            yield return null;
        }
    }
}
