using AncientTempleDefense.Enemies;
using UnityEngine;

namespace AncientTempleDefense.Vfx
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyCombatant))]
    public sealed class EnemyVfxController : MonoBehaviour
    {
        [Header("100 Best Effect Pack")]
        [SerializeField, InspectorName("Doğma Efekti")] private GameObject doğmaEfekti;
        [SerializeField, InspectorName("Hasar Efekti")] private GameObject hasarEfekti;
        [SerializeField, InspectorName("Saldırı Temas Efekti")] private GameObject saldırıTemasEfekti;
        [SerializeField, InspectorName("Ölüm Efekti")] private GameObject ölümEfekti;
        [SerializeField, InspectorName("Özel Efekt")] private GameObject özelEfekt;

        [Header("Sunum")]
        [SerializeField, InspectorName("Efekt Ölçeği"), Min(0.01f)] private float efektÖlçeği = 0.22f;
        [SerializeField, InspectorName("Efekt Y Ofseti")] private float efektYOfseti = 0.45f;
        [SerializeField, InspectorName("Efekt Ömrü"), Min(0.1f)] private float efektÖmrü = 1.6f;
        [SerializeField, InspectorName("Particle Sıralaması")] private int particleSıralaması = 16;

        private EnemyCombatant _combatant;
        private Collider2D _collider;
        private int _lastHealth;

        public GameObject SpawnEffectPrefab => doğmaEfekti;
        public GameObject HitEffectPrefab => hasarEfekti;
        public GameObject AttackEffectPrefab => saldırıTemasEfekti;
        public GameObject DeathEffectPrefab => ölümEfekti;
        public GameObject SpecialEffectPrefab => özelEfekt;
        public int SpawnedEffectCount { get; private set; }
        public float EffectScale => efektÖlçeği;

        private void Awake()
        {
            _combatant = GetComponent<EnemyCombatant>();
            _collider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            _lastHealth = _combatant != null ? _combatant.RemainingHits : 0;
            if (_combatant != null)
            {
                _combatant.HealthChanged += OnHealthChanged;
                _combatant.Died += OnDied;
            }
        }

        private void Start()
        {
            Play(doğmaEfekti, SelfEffectPosition());
        }

        private void OnDisable()
        {
            if (_combatant != null)
            {
                _combatant.HealthChanged -= OnHealthChanged;
                _combatant.Died -= OnDied;
            }
        }

        public void PlayAttackImpact(Vector3 targetPosition)
        {
            Play(saldırıTemasEfekti, targetPosition + Vector3.up * efektYOfseti);
        }

        public void PlaySpecial(Vector3 position, float scaleMultiplier = 1f)
        {
            Play(özelEfekt, position + Vector3.up * efektYOfseti, scaleMultiplier);
        }

        private void OnHealthChanged(EnemyCombatant combatant, int currentHealth, int maximumHealth)
        {
            if (currentHealth > 0 && currentHealth < _lastHealth)
            {
                Play(hasarEfekti, SelfEffectPosition());
            }

            _lastHealth = currentHealth;
        }

        private void OnDied(EnemyCombatant combatant)
        {
            Play(ölümEfekti, SelfEffectPosition());
        }

        private Vector3 SelfEffectPosition()
        {
            return _collider != null ? _collider.bounds.center : transform.position + Vector3.up * efektYOfseti;
        }

        private void Play(GameObject prefab, Vector3 position, float scaleMultiplier = 1f)
        {
            if (prefab == null) return;
            RuntimeVfxPool.Play(
                prefab,
                position,
                efektÖlçeği * Mathf.Max(0.01f, scaleMultiplier),
                particleSıralaması,
                efektÖmrü);
            SpawnedEffectCount++;
        }
    }
}
