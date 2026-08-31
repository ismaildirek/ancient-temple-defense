using AncientTempleDefense.Combat;
using NUnit.Framework;

namespace AncientTempleDefense.Tests
{
    public sealed class ThreeHitHealthTests
    {
        [Test]
        public void ThirdHitKillsEnemy()
        {
            ThreeHitHealth health = new();

            Assert.That(health.ApplyHit(), Is.False);
            Assert.That(health.ApplyHit(), Is.False);
            Assert.That(health.ApplyHit(), Is.True);
            Assert.That(health.IsDead, Is.True);
            Assert.That(health.RemainingHits, Is.Zero);
        }

        [Test]
        public void FurtherHitsDoNotChangeDeadEnemy()
        {
            ThreeHitHealth health = new();
            health.ApplyHit();
            health.ApplyHit();
            health.ApplyHit();

            Assert.That(health.ApplyHit(), Is.False);
            Assert.That(health.RemainingHits, Is.Zero);
        }

        [Test]
        public void ResetRestoresAllThreeHits()
        {
            ThreeHitHealth health = new();
            health.ApplyHit();
            health.Reset();

            Assert.That(health.RemainingHits, Is.EqualTo(3));
            Assert.That(health.IsDead, Is.False);
        }

        [Test]
        public void DamageCanConsumeMultipleHitsWithoutGoingBelowZero()
        {
            ThreeHitHealth health = new();

            Assert.That(health.ApplyDamage(2), Is.False);
            Assert.That(health.RemainingHits, Is.EqualTo(1));
            Assert.That(health.ApplyDamage(3), Is.True);
            Assert.That(health.RemainingHits, Is.Zero);
        }

        [Test]
        public void NonPositiveDamageDoesNothing()
        {
            ThreeHitHealth health = new();

            Assert.That(health.ApplyDamage(0), Is.False);
            Assert.That(health.RemainingHits, Is.EqualTo(3));
        }
    }
}
