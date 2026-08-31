using System.Collections;
using AncientTempleDefense.Scene;
using AncientTempleDefense.Vfx;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AncientTempleDefense.Tests
{
    public sealed class GameplayEnvironmentPlayModeTests
    {
        [UnityTest]
        public IEnumerator MapStartsWithVisibleEnvironmentAndBothPortalsReactToSpawns()
        {
            yield return SceneManager.LoadSceneAsync("Map", LoadSceneMode.Single);
            yield return null;

            GameplaySceneMarker marker = Object.FindFirstObjectByType<GameplaySceneMarker>();
            Transform presentation = marker.transform.Find("EnvironmentPresentation");
            Assert.That(presentation, Is.Not.Null);

            SpriteRenderer foreground = presentation.Find("ArenaForeground").GetComponent<SpriteRenderer>();
            SpriteRenderer ward = presentation.Find("TempleWardSeal").GetComponent<SpriteRenderer>();
            AmbientSpritePulse left = presentation.Find("EnemyPortalLeft").GetComponent<AmbientSpritePulse>();
            AmbientSpritePulse right = presentation.Find("EnemyPortalRight").GetComponent<AmbientSpritePulse>();

            Assert.That(foreground.enabled, Is.True);
            Assert.That(foreground.sprite, Is.Not.Null);
            Assert.That(ward.enabled, Is.True);
            Assert.That(ward.sprite, Is.Not.Null);
            Assert.That(left, Is.Not.Null);
            Assert.That(right, Is.Not.Null);

            float portalDeadline = Time.realtimeSinceStartup + 3f;
            while ((left.BurstCount < 1 || right.BurstCount < 1)
                   && Time.realtimeSinceStartup < portalDeadline)
            {
                yield return null;
            }

            Assert.That(left.BurstCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(right.BurstCount, Is.GreaterThanOrEqualTo(1));
        }
    }
}
