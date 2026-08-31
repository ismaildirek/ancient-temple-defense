using System.Collections;
using AncientTempleDefense.Audio;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class GameplayAudioPlayModeTests
    {
        [UnityTest]
        public IEnumerator MapStartsMusicAndCombatActionsSelectCorrectAudioPacks()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            BattleMusicPlayer music = Object.FindFirstObjectByType<BattleMusicPlayer>();
            Assert.That(music, Is.Not.Null);
            Assert.That(music.IsConfigured, Is.True);
            Assert.That(music.MusicClip.name, Is.EqualTo("Battle Theme 1_demo"));

            BlackKnightPlayerController player = Object.FindFirstObjectByType<BlackKnightPlayerController>();
            BlackKnightSwordAudio playerAudio = player.GetComponent<BlackKnightSwordAudio>();
            Assert.That(playerAudio, Is.Not.Null);
            playerAudio.PlayAttack(false, 0.65f, 0.42f);
            Assert.That(playerAudio.LastPlayedClip, Is.Not.Null);
            Assert.That(playerAudio.LastPlayedClip.name, Does.StartWith("SWORD_"));
            Assert.That(playerAudio.LastScheduledDuration, Is.GreaterThan(0.70f));

            yield return new WaitForSeconds(2.2f);
            EnemyCombatant enemy = Object.FindFirstObjectByType<EnemyCombatant>();
            EnemyAudioController enemyAudio = enemy.GetComponent<EnemyAudioController>();
            Assert.That(enemyAudio, Is.Not.Null);

            enemy.TakeHit();
            Assert.That(enemyAudio.LastPlayedClip, Is.Not.Null);
            Assert.That(enemyAudio.LastPlayedClip.name, Does.Not.StartWith("SWORD_"));
        }
    }
}
