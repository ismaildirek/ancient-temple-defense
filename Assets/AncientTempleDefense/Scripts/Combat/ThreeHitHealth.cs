namespace AncientTempleDefense.Combat
{
    public sealed class ThreeHitHealth
    {
        public ThreeHitHealth(int requiredHits = 3)
        {
            RequiredHits = requiredHits < 1 ? 1 : requiredHits;
            RemainingHits = RequiredHits;
        }

        public int RequiredHits { get; }

        public int RemainingHits { get; private set; }

        public bool IsDead => RemainingHits == 0;

        public bool ApplyHit()
        {
            return ApplyDamage(1);
        }

        public bool ApplyDamage(int damage)
        {
            if (IsDead || damage <= 0)
            {
                return false;
            }

            RemainingHits = damage >= RemainingHits ? 0 : RemainingHits - damage;
            return IsDead;
        }

        public void Reset()
        {
            RemainingHits = RequiredHits;
        }
    }
}
