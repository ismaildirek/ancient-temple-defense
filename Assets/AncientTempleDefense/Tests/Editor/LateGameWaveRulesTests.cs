using AncientTempleDefense.Enemies;
using NUnit.Framework;

namespace AncientTempleDefense.Tests
{
    public sealed class LateGameWaveRulesTests
    {
        [TestCase(1, false)]
        [TestCase(7, false)]
        [TestCase(8, true)]
        [TestCase(20, true)]
        public void LateEnemiesUnlockFromWaveEight(int wave, bool expected)
        {
            Assert.That(WaveEnemyRosterRules.LateEnemiesUnlocked(wave, 8), Is.EqualTo(expected));
        }

        [TestCase(6, false)]
        [TestCase(7, true)]
        [TestCase(8, false)]
        [TestCase(12, true)]
        [TestCase(13, false)]
        public void BossWavesAreSevenAndTwelve(int wave, bool expected)
        {
            Assert.That(WaveEnemyRosterRules.IsBossWave(wave, 7, 12), Is.EqualTo(expected));
        }
    }
}
