using UnityEngine;
using UnityEngine.Serialization;

namespace AncientTempleDefense.Vfx
{
[DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class AmbientSpritePulse : MonoBehaviour
    {
        [Header("Idle Pulse")]
        [FormerlySerializedAs("cyclesPerSecond")]
        [SerializeField, Min(0f)] private float saniyedekiDöngü = 0.55f;
        [FormerlySerializedAs("scaleAmplitude")]
        [SerializeField, Range(0f, 0.25f)] private float ölçekGenliği = 0.035f;
        [FormerlySerializedAs("alphaAmplitude")]
        [SerializeField, Range(0f, 0.5f)] private float saydamlıkGenliği = 0.08f;
        [FormerlySerializedAs("phaseOffset")]
        [SerializeField] private float fazKayması;

        [Header("Spawn Burst")]
        [FormerlySerializedAs("burstDuration")]
        [SerializeField, Min(0.05f)] private float parlamaSüresi = 0.3f;
        [FormerlySerializedAs("burstScaleBonus")]
        [SerializeField, Range(0f, 0.5f)] private float parlamaÖlçekBonusu = 0.14f;
        [FormerlySerializedAs("burstBrightnessBonus")]
        [SerializeField, Range(0f, 1f)] private float parlamaParlaklıkBonusu = 0.22f;

        private SpriteRenderer _renderer;
        private Vector3 _baseScale;
private Color _baseColor;
        private float _burstEndsAt;
        private bool _initialized;

        public int BurstCount { get; private set; }

        private void Awake()
        {
            CaptureBaseState();
        }

        private void OnEnable()
        {
            if (!_initialized)
            {
                CaptureBaseState();
            }

            BurstCount = 0;
            _burstEndsAt = 0f;
        }

        private void OnDisable()
        {
            RestoreBaseState();
        }

        private void Update()
        {
            float phase = (Time.time * saniyedekiDöngü + fazKayması) * Mathf.PI * 2f;
            float pulse = (Mathf.Sin(phase) + 1f) * 0.5f;
            float burst = parlamaSüresi > 0f
                ? Mathf.Clamp01((_burstEndsAt - Time.time) / parlamaSüresi)
                : 0f;

            transform.localScale = _baseScale * (1f + pulse * ölçekGenliği + burst * parlamaÖlçekBonusu);

            Color color = _baseColor;
            color.a = Mathf.Clamp01(_baseColor.a - pulse * saydamlıkGenliği + burst * saydamlıkGenliği);
            color.r = Mathf.Clamp01(color.r + burst * parlamaParlaklıkBonusu);
            color.g = Mathf.Clamp01(color.g + burst * parlamaParlaklıkBonusu);
            color.b = Mathf.Clamp01(color.b + burst * parlamaParlaklıkBonusu);
            _renderer.color = color;
        }

        public void Burst()
        {
            BurstCount++;
            _burstEndsAt = Time.time + parlamaSüresi;
        }

        private void CaptureBaseState()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
            _baseColor = _renderer.color;
            _initialized = true;
        }

        private void RestoreBaseState()
        {
            if (!_initialized)
            {
                return;
            }

            transform.localScale = _baseScale;
            _renderer.color = _baseColor;
        }
    }
}
