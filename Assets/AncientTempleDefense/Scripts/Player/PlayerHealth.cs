using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientTempleDefense.Player
{
[DisallowMultipleComponent]
    [RequireComponent(typeof(BlackKnightPlayerController))]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [Header("Oyuncu Dayanıklılığı")]
        [FormerlySerializedAs("maximumHealth")]
        [SerializeField, InspectorName("Azami Can"), Min(1)] private int azamiCan = 100;
        [FormerlySerializedAs("armor")]
        [SerializeField, InspectorName("Başlangıç Zırhı"), Min(0)] private int zırh;
        [FormerlySerializedAs("hitInvulnerability")]
        [SerializeField, InspectorName("Hasar Sonrası Koruma"), Min(0f)] private float hasarSonrasıKoruma = 0.35f;

        private BlackKnightPlayerController _controller;
        private float _nextDamageAt;
        public event Action<int, int> HealthChanged;
        public event Action Died;

        public int MaximumHealth => azamiCan;
        public int CurrentHealth { get; private set; }
        public int Armor => zırh;
        public bool IsDead => CurrentHealth <= 0;

        private void Awake()
        {
            _controller = GetComponent<BlackKnightPlayerController>();
            CurrentHealth = azamiCan;
        }

        private void OnEnable()
        {
            if (CurrentHealth <= 0)
            {
                CurrentHealth = azamiCan;
            }
        }
        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead || Time.unscaledTime < _nextDamageAt)
            {
                return;
            }

            float armorReduction = zırh / (zırh + 100f);
            float defenseReduction = _controller != null && _controller.IsDefending ? 0.5f : 1f;
            int resolved = Mathf.Max(1, Mathf.CeilToInt(amount * (1f - armorReduction) * defenseReduction));
            CurrentHealth = Mathf.Max(0, CurrentHealth - resolved);
            _nextDamageAt = Time.unscaledTime + hasarSonrasıKoruma;
            HealthChanged?.Invoke(CurrentHealth, azamiCan);

            if (IsDead)
            {                Died?.Invoke();
                if (_controller != null)
                {
                    _controller.enabled = false;
                }
            }
        }

        public void IncreaseMaximumHealth(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            azamiCan += amount;
            CurrentHealth = Mathf.Min(azamiCan, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, azamiCan);
        }

        public int CanYenile(int miktar)
        {
            if (miktar <= 0 || IsDead || CurrentHealth >= MaximumHealth)
            {
                return 0;
            }

            int oncekiCan = CurrentHealth;
            CurrentHealth = Mathf.Min(MaximumHealth, CurrentHealth + miktar);
            int yenilenenCan = CurrentHealth - oncekiCan;
            HealthChanged?.Invoke(CurrentHealth, MaximumHealth);
            return yenilenenCan;
        }

        public void IncreaseArmor(int amount)
        {
            zırh = Mathf.Max(0, zırh + amount);
        }
    }
}