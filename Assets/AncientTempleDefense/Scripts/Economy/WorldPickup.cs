using AncientTempleDefense.Player;
using AncientTempleDefense.Temple;
using UnityEngine;

namespace AncientTempleDefense.Economy
{
    public enum PickupType
    {
        Coin,
        HealthPotion,
        TemplePotion
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public sealed class WorldPickup : MonoBehaviour
    {
        [SerializeField, InspectorName("Esya Turu")]
        private PickupType esyaTuru;

        [SerializeField, InspectorName("Deger"), Min(1)]
        private int deger = 1;

        [SerializeField, InspectorName("Toplanma Gecikmesi"), Min(0f)]
        private float toplanmaGecikmesi = 0.12f;

        [SerializeField, InspectorName("Havada Salinma"), Min(0f)]
        private float havadaSalinma = 0.08f;

        [SerializeField, InspectorName("Dunyada Kalma Suresi"), Min(5f)]
        private float dunyadaKalmaSuresi = 15f;

        private float _toplanabilirZaman;
        private float _kaybolmaZamani;
        private float _salinmaFazi;
        private bool _toplandi;
        private Vector3 _baslangicKonumu;
        private static TempleHealth _cachedTemple;

        public PickupType Type => esyaTuru;
        public int Value => deger;

        private void OnEnable()
        {
            _baslangicKonumu = transform.position;
            _toplanabilirZaman = Time.time + toplanmaGecikmesi;
            _kaybolmaZamani = Time.time + Mathf.Max(5f, dunyadaKalmaSuresi);
            _salinmaFazi = GetInstanceID() * 0.01f;
            _toplandi = false;
        }

        private void Update()
        {
            if (Time.time >= _kaybolmaZamani)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 position = _baslangicKonumu;
            position.y += Mathf.Sin(Time.time * 4.5f + _salinmaFazi) * havadaSalinma;
            transform.position = position;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryCollect(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryCollect(other);
        }

        private void TryCollect(Collider2D other)
        {
            if (_toplandi || Time.time < _toplanabilirZaman)
            {
                return;
            }

            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null)
            {
                return;
            }

            if (esyaTuru == PickupType.Coin)
            {
                PlayerWallet wallet = playerHealth.GetComponent<PlayerWallet>();
                if (wallet == null)
                {
                    return;
                }

                _toplandi = true;
                wallet.CoinEkle(deger);
                Destroy(gameObject);
                return;
            }
            if (esyaTuru == PickupType.TemplePotion)
            {
                TempleHealth temple = _cachedTemple != null ? _cachedTemple : (_cachedTemple = FindFirstObjectByType<TempleHealth>());
                if (temple != null && temple.CanYenile(deger) > 0)
                {
                    _toplandi = true;
                    Destroy(gameObject);
                }
                return;
            }

            if (playerHealth.CanYenile(deger) > 0)
            {
                _toplandi = true;
                Destroy(gameObject);
            }
        }
    }
}
