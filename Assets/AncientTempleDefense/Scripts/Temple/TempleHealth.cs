using System;
using System.Collections;
using UnityEngine;

namespace AncientTempleDefense.Temple
{
    public enum TempleDamageStage
    {
        Sağlam,
        Hasarlı,
        Kritik
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TempleHealth : MonoBehaviour
    {
        [Header("Tapınak Dayanıklılığı")]
        [SerializeField, InspectorName("Azami Can"), Min(1)] private int azamiCan = 2000;
        [SerializeField, InspectorName("Hasarlı Aşama Eşiği"), Range(0.35f, 0.9f)] private float hasarlıAşamaEşiği = 0.66f;
        [SerializeField, InspectorName("Kritik Aşama Eşiği"), Range(0.05f, 0.5f)] private float kritikAşamaEşiği = 0.33f;

        [Header("Hasar Görünümü")]
        [SerializeField, InspectorName("Ana Tapınak Görseli")] private SpriteRenderer anaGörsel;
        [SerializeField, InspectorName("Tapınak Mührü Görseli")] private SpriteRenderer mühürGörseli;
        [SerializeField, InspectorName("Hasarlı Renk")] private Color hasarlıRenk = new(1f, 0.68f, 0.48f, 1f);
        [SerializeField, InspectorName("Kritik Renk")] private Color kritikRenk = new(0.95f, 0.28f, 0.20f, 1f);
        [SerializeField, InspectorName("Yıkılmış Renk")] private Color yıkılmışRenk = new(0.28f, 0.20f, 0.24f, 1f);
        [SerializeField, InspectorName("Hasar Parlama Süresi"), Min(0.01f)] private float hasarParlamaSüresi = 0.12f;

        private int _currentHealth;
        private Color _healthyMainColor = Color.white;
        private Color _healthySealColor = Color.white;
        private Coroutine _damageFlash;

        public event Action<int, int, TempleDamageStage> HealthChanged;
        public event Action Destroyed;

        public int CurrentHealth => _currentHealth;
        public int MaximumHealth => Mathf.Max(1, azamiCan);
        public bool IsDestroyed => _currentHealth <= 0;
        public TempleDamageStage DamageStage { get; private set; } = TempleDamageStage.Sağlam;

        private void Awake()
        {
            if (anaGörsel == null)
            {
                anaGörsel = GetComponent<SpriteRenderer>();
            }

            _healthyMainColor = anaGörsel != null ? anaGörsel.color : Color.white;
            _healthySealColor = mühürGörseli != null ? mühürGörseli.color : Color.white;
            _currentHealth = MaximumHealth;
            ApplyDamageAppearance();
        }

        private void OnDisable()
        {
            if (_damageFlash != null)
            {
                StopCoroutine(_damageFlash);
                _damageFlash = null;
            }
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDestroyed)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            DamageStage = CalculateStage(_currentHealth, MaximumHealth, hasarlıAşamaEşiği, kritikAşamaEşiği);
            ApplyDamageAppearance();
            HealthChanged?.Invoke(_currentHealth, MaximumHealth, DamageStage);

            if (_currentHealth == 0)
            {
                Destroyed?.Invoke();
                return;
            }

            if (_damageFlash != null)
            {
                StopCoroutine(_damageFlash);
            }

            _damageFlash = StartCoroutine(DamageFlash());
        }

        public int CanYenile(int miktar)
        {
            if (miktar <= 0 || IsDestroyed || _currentHealth >= MaximumHealth)
            {
                return 0;
            }

            int oncekiCan = _currentHealth;
            _currentHealth = Mathf.Min(MaximumHealth, _currentHealth + miktar);
            DamageStage = CalculateStage(_currentHealth, MaximumHealth, hasarlıAşamaEşiği, kritikAşamaEşiği);
            ApplyDamageAppearance();
            HealthChanged?.Invoke(_currentHealth, MaximumHealth, DamageStage);
            return _currentHealth - oncekiCan;
        }

        public static TempleDamageStage CalculateStage(
            int currentHealth,
            int maximumHealth,
            float damagedThreshold = 0.66f,
            float criticalThreshold = 0.33f)
        {
            float ratio = maximumHealth > 0 ? Mathf.Clamp01((float)currentHealth / maximumHealth) : 0f;
            if (ratio <= Mathf.Clamp01(criticalThreshold))
            {
                return TempleDamageStage.Kritik;
            }

            return ratio <= Mathf.Clamp01(damagedThreshold)
                ? TempleDamageStage.Hasarlı
                : TempleDamageStage.Sağlam;
        }

        private IEnumerator DamageFlash()
        {
            if (anaGörsel != null)
            {
                anaGörsel.color = Color.white;
            }

            if (mühürGörseli != null)
            {
                mühürGörseli.color = Color.white;
            }

            yield return new WaitForSeconds(hasarParlamaSüresi);
            _damageFlash = null;
            ApplyDamageAppearance();
        }

        private void ApplyDamageAppearance()
        {
            Color mainColor = IsDestroyed
                ? yıkılmışRenk
                : DamageStage switch
                {
                    TempleDamageStage.Hasarlı => hasarlıRenk,
                    TempleDamageStage.Kritik => kritikRenk,
                    _ => _healthyMainColor
                };

            Color sealColor = IsDestroyed
                ? new Color(yıkılmışRenk.r, yıkılmışRenk.g, yıkılmışRenk.b, 0.35f)
                : DamageStage switch
                {
                    TempleDamageStage.Hasarlı => new Color(1f, 0.48f, 0.20f, 1f),
                    TempleDamageStage.Kritik => new Color(1f, 0.12f, 0.08f, 1f),
                    _ => _healthySealColor
                };

            if (anaGörsel != null)
            {
                anaGörsel.color = mainColor;
            }

            if (mühürGörseli != null)
            {
                mühürGörseli.color = sealColor;
            }
        }
    }
}
