using System.Collections;
using System.Linq;
using System.Reflection;
using AncientTempleDefense.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class KeyboardInputPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator KeyboardDMovementAndDigitOneBindingAreConnected()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            Assert.That(player, Is.Not.Null);
            yield return new WaitForSeconds(0.6f);
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();

            float startX = player.transform.position.x;
            Press(keyboard.dKey);
            yield return null;
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(player.transform.position.x, Is.GreaterThan(startX));
            Release(keyboard.dKey);

            FieldInfo lightAttackField = typeof(BlackKnightPlayerController).GetField(
                "_lightAttackAction",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(lightAttackField, Is.Not.Null);
            InputAction lightAttack = lightAttackField.GetValue(player) as InputAction;
            Assert.That(lightAttack, Is.Not.Null);
            Assert.That(lightAttack.enabled, Is.True);
            Assert.That(lightAttack.bindings.Any(binding => binding.effectivePath == "<Keyboard>/1"), Is.True);
        }
        [UnityTest]
        public IEnumerator WasdInterruptsAttackAndStartsMovementImmediately()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return new WaitForSeconds(1.5f);

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            Animator animator = player.GetComponent<Animator>();

            Press(keyboard.digit1Key);
            yield return null;
            Assert.That(player.IsAttacking, Is.True, "Hafif saldırı başlamalı.");

            float beforeMovementX = player.transform.position.x;
            Press(keyboard.dKey);
            yield return null;
            Assert.That(player.IsAttacking, Is.False, "D hareketi saldırıyı aynı karede kesmeli.");

            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(player.transform.position.x, Is.GreaterThan(beforeMovementX));
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("BK_weapon_run"), Is.True);
            Release(keyboard.dKey);
            Release(keyboard.digit1Key);
        }
        [UnityTest]
        public IEnumerator WasdDoesNotInterruptUltimateAndMovementRemainsLocked()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return new WaitForSeconds(1.5f);

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            Assert.That(player, Is.Not.Null);

            Press(keyboard.digit4Key);
            yield return null;
            Release(keyboard.digit4Key);
            Assert.That(player.IsUltimateInProgress, Is.True, "Ulti başlamalı.");

            float lockedX = player.transform.position.x;
            Press(keyboard.dKey);
            yield return null;
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(player.IsUltimateInProgress, Is.True, "WASD ultiyi iptal etmemeli.");
            Assert.That(player.IsAttacking, Is.True, "Ulti saldırı durumu korunmalı.");
            Assert.That(player.transform.position.x, Is.EqualTo(lockedX).Within(0.01f), "Ulti sırasında yatay hareket kilitli olmalı.");

            Release(keyboard.dKey);
            float deadline = Time.realtimeSinceStartup + 6f;
            while (player.IsUltimateInProgress && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(player.IsUltimateInProgress, Is.False, "Ulti animasyonu tamamlanmalı.");
        }
    }
}