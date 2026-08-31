using AncientTempleDefense.Enemies;
using UnityEngine;

namespace AncientTempleDefense.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyCombatant))]
    public sealed class EnemyLootDropper : MonoBehaviour
    {
        [Header("Dusme Ihtimalleri")]
        [SerializeField, InspectorName("Coin Dusme Ihtimali"), Range(0f, 1f)]
        private float coinDusmeIhtimali = 0.70f;

        [SerializeField, InspectorName("Iksir Dusme Ihtimali"), Range(0f, 1f)]
        private float iksirDusmeIhtimali = 0.50f;
        [SerializeField, InspectorName("Tapinak Iksiri Dusme Ihtimali"), Range(0f, 1f)]
        private float tapinakIksiriDusmeIhtimali = 0.10f;

        [Header("Esya Prefablari")]
        [SerializeField, InspectorName("Coin Prefabi")]
        private WorldPickup coinPrefabi;

        [SerializeField, InspectorName("Can Iksiri Prefabi")]
        private WorldPickup canIksiriPrefabi;
        [SerializeField, InspectorName("Tapinak Iksiri Prefabi")]
        private WorldPickup tapinakIksiriPrefabi;

        private EnemyCombatant _combatant;
        private bool _dropped;

        public float CoinDropChance => coinDusmeIhtimali;
        public float PotionDropChance => iksirDusmeIhtimali;

        private void Awake()
        {
            _combatant = GetComponent<EnemyCombatant>();
        }

        private void OnEnable()
        {
            _dropped = false;
            if (_combatant == null)
            {
                _combatant = GetComponent<EnemyCombatant>();
            }

            _combatant.Died += OnDied;
        }

        private void OnDisable()
        {
            if (_combatant != null)
            {
                _combatant.Died -= OnDied;
            }
        }

        public static bool DusurulurMu(float rastgeleDeger, float ihtimal)
        {
            return rastgeleDeger < Mathf.Clamp01(ihtimal);
        }

        private void OnDied(EnemyCombatant enemy)
        {
            if (_dropped)
            {
                return;
            }

            _dropped = true;
            Vector3 dropPosition = transform.position;
            dropPosition.y += 0.18f;

            if (coinPrefabi != null && DusurulurMu(Random.value, coinDusmeIhtimali))
            {
                Instantiate(coinPrefabi, dropPosition + Vector3.left * 0.16f, Quaternion.identity);
            }

            if (canIksiriPrefabi != null && DusurulurMu(Random.value, iksirDusmeIhtimali))
            {
                Instantiate(canIksiriPrefabi, dropPosition + Vector3.right * 0.16f, Quaternion.identity);
            }
            if (tapinakIksiriPrefabi != null && DusurulurMu(Random.value, tapinakIksiriDusmeIhtimali))
            {
                Instantiate(tapinakIksiriPrefabi, dropPosition + Vector3.up * 0.18f, Quaternion.identity);
            }
        }
    }
}
