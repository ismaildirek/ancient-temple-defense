using UnityEngine;

namespace AncientTempleDefense.Progression
{
    public static class WaveScaling
    {
        public static int EnemyCount(int wave, int baseCount, int additionalPerWave)
        {
            int index = Mathf.Max(0, wave - 1);
            return Mathf.Max(1, baseCount + index * Mathf.Max(0, additionalPerWave));
        }

        public static int EnemyHealth(int wave, int baseHealth, float growthPerWave)
        {
            return ScaleWholeNumber(wave, baseHealth, growthPerWave);
        }

        public static int EnemyDamage(int wave, int baseDamage, float growthPerWave)
        {
            return ScaleWholeNumber(wave, baseDamage, growthPerWave);
        }

        public static float AttackSpeedMultiplier(int wave, float growthPerWave)
        {
            int index = Mathf.Max(0, wave - 1);
            return Mathf.Max(0.1f, 1f + Mathf.Max(0f, growthPerWave) * index);
        }

        private static int ScaleWholeNumber(int wave, int baseValue, float growthPerWave)
        {
            int index = Mathf.Max(0, wave - 1);
            float multiplier = 1f + Mathf.Max(0f, growthPerWave) * index;
            return Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(1, baseValue) * multiplier));
        }
    }
}
