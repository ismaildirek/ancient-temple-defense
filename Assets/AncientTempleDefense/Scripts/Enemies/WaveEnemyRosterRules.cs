namespace AncientTempleDefense.Enemies
{
    public static class WaveEnemyRosterRules
    {
        public static bool LateEnemiesUnlocked(int wave, int unlockWave)
        {
            return wave >= System.Math.Max(1, unlockWave);
        }

        public static bool IsBossWave(int wave, int firstBossWave, int secondBossWave)
        {
            return wave == firstBossWave || wave == secondBossWave;
        }
    }
}
