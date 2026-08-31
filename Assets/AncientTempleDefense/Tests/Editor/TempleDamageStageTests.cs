using AncientTempleDefense.Temple;
using NUnit.Framework;

namespace AncientTempleDefense.Tests
{
    public sealed class TempleDamageStageTests
    {
        [TestCase(1000, 1000, TempleDamageStage.Sağlam)]
        [TestCase(661, 1000, TempleDamageStage.Sağlam)]
        [TestCase(660, 1000, TempleDamageStage.Hasarlı)]
        [TestCase(331, 1000, TempleDamageStage.Hasarlı)]
        [TestCase(330, 1000, TempleDamageStage.Kritik)]
        [TestCase(0, 1000, TempleDamageStage.Kritik)]
        public void HealthRatioSelectsExpectedDamageStage(
            int currentHealth,
            int maximumHealth,
            TempleDamageStage expectedStage)
        {
            Assert.That(TempleHealth.CalculateStage(currentHealth, maximumHealth), Is.EqualTo(expectedStage));
        }
    }
}
