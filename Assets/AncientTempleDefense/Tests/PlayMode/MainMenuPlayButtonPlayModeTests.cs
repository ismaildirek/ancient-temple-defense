using System.Collections;
using AncientTempleDefense.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class MainMenuPlayButtonPlayModeTests
    {
        [UnityTest]
        public IEnumerator PlayButtonLoadsGameplayMap()
        {
            yield return SceneManager.LoadSceneAsync("giris", LoadSceneMode.Single);
            yield return null;

            MainMenuPlayButton playButton = Object.FindFirstObjectByType<MainMenuPlayButton>();
            Assert.That(playButton, Is.Not.Null);
            Assert.That(playButton.TryStartGame(), Is.True);

            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Map"));
        }
    }
}
