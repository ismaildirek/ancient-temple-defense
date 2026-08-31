using System;
using UnityEngine;

namespace AncientTempleDefense.Economy
{
    [DisallowMultipleComponent]
    public sealed class PlayerWallet : MonoBehaviour
    {
        [SerializeField, InspectorName("Baslangic Coini"), Min(0)]
        private int baslangicCoini;

        public event Action<int> CoinChanged;
        public int Coin { get; private set; }

        private void Awake()
        {
            Coin = Mathf.Max(0, baslangicCoini);
        }

        public void CoinEkle(int miktar)
        {
            if (miktar <= 0)
            {
                return;
            }

            Coin += miktar;
            CoinChanged?.Invoke(Coin);
        }

        public bool Harca(int miktar)
        {
            if (miktar < 0 || Coin < miktar)
            {
                return false;
            }

            Coin -= miktar;
            CoinChanged?.Invoke(Coin);
            return true;
        }
    }
}
