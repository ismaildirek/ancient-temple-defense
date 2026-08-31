using UnityEngine;

namespace AncientTempleDefense.Enemies
{
    public enum EnemyTargetMode
    {
        NearestThreat = 0,
        PlayerOnly = 1,
        TempleOnly = 2,
        DefendersOnly = 3
    }

    [DisallowMultipleComponent]
    public sealed class EnemyRoleProfile : MonoBehaviour
    {
        [Header("Hedef Davranışı")]
        [SerializeField, InspectorName("Hedef Önceliği")] private EnemyTargetMode hedefÖnceliği;
        [SerializeField, InspectorName("Uçan Düşman")] private bool uçanDüşman;

        [Header("Wave Çarpanları")]
        [SerializeField, InspectorName("Can Çarpanı"), Min(0.1f)] private float canÇarpanı = 1f;
        [SerializeField, InspectorName("Hasar Çarpanı"), Min(0.1f)] private float hasarÇarpanı = 1f;
        [SerializeField, InspectorName("Hareket Hızı Çarpanı"), Min(0.1f)] private float hareketHızıÇarpanı = 1f;
        [SerializeField, InspectorName("Vuruş Hızı Çarpanı"), Min(0.1f)] private float vuruşHızıÇarpanı = 1f;

        public EnemyTargetMode TargetMode => hedefÖnceliği;
        public bool IsFlying => uçanDüşman;
        public float HealthMultiplier => Mathf.Max(0.1f, canÇarpanı);
        public float DamageMultiplier => Mathf.Max(0.1f, hasarÇarpanı);
        public float MovementSpeedMultiplier => Mathf.Max(0.1f, hareketHızıÇarpanı);
        public float AttackSpeedMultiplier => Mathf.Max(0.1f, vuruşHızıÇarpanı);
    }
}
