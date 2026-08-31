using System;
using System.Collections;
using System.Collections.Generic;
using AncientTempleDefense.Animation;
using AncientTempleDefense.Audio;
using AncientTempleDefense.Combat;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientTempleDefense.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Collider2D))]
    public sealed class EnemyCombatant : MonoBehaviour
    {
        [Header("Animasyon Durumlar\u0131")]
        [FormerlySerializedAs("idleState")]
        [SerializeField, InspectorName("Bekleme Animasyonu")] private string beklemeAnimasyonu = "Idle";
        [FormerlySerializedAs("hitState")]
        [SerializeField, InspectorName("Hasar Alma Animasyonu")] private string hasarAlmaAnimasyonu = "Hit";
        [FormerlySerializedAs("deathState")]
        [SerializeField, InspectorName("\u00d6l\u00fcm Animasyonu")] private string \u00f6l\u00fcmAnimasyonu = "Death";

        [Header("Dayan\u0131kl\u0131l\u0131k")]
        [FormerlySerializedAs("requiredHits")]
        [SerializeField, InspectorName("Gerekli Vuru\u015f / Can"), Min(1)] private int gerekliVuru\u015fSay\u0131s\u0131 = 3;

        [Header("Sahne Yerle\u015fimi")]
        [SerializeField, InspectorName("Do\u011fma Y Ofseti")]
        private float do\u011fmaYOfseti;

        private static readonly List<EnemyCombatant> Active = new();
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;
        private Rigidbody2D _body;
        private AnimationStatePlayer _animations;
        private EnemyAudioController _audio;
        private ThreeHitHealth _health;
        private EnemyBrain _enemyBrain;
        private BossEnemyBrain _bossBrain;
        private float _actionLockedUntil;
        private float _hitAnimationLockedUntil;
        private Color _persistentTint = Color.white;

        public static IReadOnlyList<EnemyCombatant> ActiveEnemies => Active;
        public static event Action<EnemyCombatant> AnyEnemyDied;
        public event Action<EnemyCombatant> Died;
        public event Action<EnemyCombatant, int, int> HealthChanged;
        public bool IsDead => _health != null && _health.IsDead;
        public bool IsActionLocked => Time.time < _actionLockedUntil;
        public int RemainingHits => _health?.RemainingHits ?? gerekliVuru\u015fSay\u0131s\u0131;
        public int MaximumHits => _health?.RequiredHits ?? gerekliVuru\u015fSay\u0131s\u0131;
        public float SpawnYOffset => do\u011fmaYOfseti;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            Active.Clear();
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _body = GetComponent<Rigidbody2D>();
            _animations = new AnimationStatePlayer(GetComponent<Animator>());
            _audio = GetComponent<EnemyAudioController>();
            _enemyBrain = GetComponent<EnemyBrain>();
            _bossBrain = GetComponent<BossEnemyBrain>();
            _health = new ThreeHitHealth(gerekliVuru\u015fSay\u0131s\u0131);
        }

        private void OnEnable()
        {
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }

            _health?.Reset();
            _actionLockedUntil = 0f;
            _hitAnimationLockedUntil = 0f;
            _persistentTint = Color.white;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _persistentTint;
            }
            if (_collider != null)
            {
                _collider.enabled = true;
            }

            _animations?.Play(beklemeAnimasyonu, true);
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        public void ConfigureForWave(int health)
        {
            gerekliVuru\u015fSay\u0131s\u0131 = Mathf.Max(1, health);
            _health = new ThreeHitHealth(gerekliVuru\u015fSay\u0131s\u0131);
            HealthChanged?.Invoke(this, RemainingHits, MaximumHits);
        }

        public void TakeHit(int damage = 1)
        {
            if (IsDead || damage <= 0)
            {
                return;
            }

            bool died = _health.ApplyDamage(damage);
            HealthChanged?.Invoke(this, RemainingHits, MaximumHits);
            if (died)
            {
                Died?.Invoke(this);
                AnyEnemyDied?.Invoke(this);
                StartCoroutine(DeathSequence());
                return;
            }

            if (_bossBrain != null && _bossBrain.IsAttacking)
            {
                _audio?.PlayHit(0.12f);
                StartCoroutine(HitFlash(0.12f));
                return;
            }
            if (Time.time >= _hitAnimationLockedUntil)
            {
                float duration = _animations.Duration(hasarAlmaAnimasyonu, 0.35f);
                _hitAnimationLockedUntil = Time.time + duration;
                _actionLockedUntil = Mathf.Max(_actionLockedUntil, _hitAnimationLockedUntil);
                _animations.Play(hasarAlmaAnimasyonu, true);
                _audio?.PlayHit(duration);
                StartCoroutine(HitFlash(duration));
            }
        }

        public bool TryPlayAction(string stateName, float fallbackDuration)
        {
            if (IsDead || IsActionLocked)
            {
                return false;
            }

            float duration = _animations.Duration(stateName, fallbackDuration);
            _actionLockedUntil = Time.time + duration;
            _animations.Play(stateName, true);
            _audio?.PlayAction(stateName, duration);
            return true;
        }

        public void PlayLoop(string stateName)
        {
            if (!IsDead && !IsActionLocked)
            {
                _animations.Play(stateName);
            }
        }

        public float AnimationDuration(string stateName, float fallback)
        {
            return _animations.Duration(stateName, fallback);
        }

        public void SetPersistentTint(Color color)
        {
            _persistentTint = color;
            if (_spriteRenderer != null && !IsDead)
            {
                _spriteRenderer.color = _persistentTint;
            }
        }

        private IEnumerator HitFlash(float duration)
        {
            _spriteRenderer.color = new Color(1f, 0.35f, 0.25f, 1f);
            yield return new WaitForSeconds(Mathf.Min(duration, 0.12f));
            if (!IsDead)
            {
                _spriteRenderer.color = _persistentTint;
            }
        }

        private IEnumerator DeathSequence()
        {
            if (_enemyBrain != null)
            {
                _enemyBrain.enabled = false;
            }

            if (_bossBrain != null)
            {
                _bossBrain.enabled = false;
            }

            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
            }

            _collider.enabled = false;
            _spriteRenderer.color = Color.white;
            float duration = _animations.Duration(\u00f6l\u00fcmAnimasyonu, 0.5f);
            float audioDuration = _audio?.PlayDeath(duration) ?? 0f;
            _actionLockedUntil = float.PositiveInfinity;
            _animations.Play(\u00f6l\u00fcmAnimasyonu, true);
            yield return new WaitForSeconds(Mathf.Max(duration + 0.08f, audioDuration));
            Destroy(gameObject);
        }
    }
}
