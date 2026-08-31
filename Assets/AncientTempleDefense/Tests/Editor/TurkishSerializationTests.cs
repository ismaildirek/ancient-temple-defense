using System.Reflection;
using AncientTempleDefense.Audio;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using NUnit.Framework;
using UnityEngine.Serialization;

namespace AncientTempleDefense.Tests
{
    public sealed class TurkishSerializationTests
    {
        [Test]
        public void TurkishFieldNamesPreserveExistingSerializedData()
        {
            AssertFormerlySerializedAs(typeof(BlackKnightPlayerController), "hareketH\u0131z\u0131", "moveSpeed");
            AssertFormerlySerializedAs(typeof(BlackKnightPlayerController), "hafifSald\u0131r\u0131Hasar\u0131", "lightAttackDamage");
            AssertFormerlySerializedAs(typeof(BlackKnightSwordAudio), "hafifSald\u0131r\u0131Sesleri", "lightAttackSounds");
            AssertFormerlySerializedAs(typeof(EnemyWaveSpawner), "waveBa\u015f\u0131naCanArt\u0131\u015f\u0131", "healthGrowthPerWave");
            AssertFormerlySerializedAs(typeof(PlayerHealth), "azamiCan", "maximumHealth");
        }

        private static void AssertFormerlySerializedAs(System.Type type, string fieldName, string oldName)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            FormerlySerializedAsAttribute migration = field.GetCustomAttribute<FormerlySerializedAsAttribute>();
            Assert.That(migration, Is.Not.Null);
            Assert.That(migration.oldName, Is.EqualTo(oldName));
        }
    }
}

