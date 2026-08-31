using System;
using System.Collections;
using System.Collections.Generic;
using AncientTempleDefense.Animation;
using AncientTempleDefense.Audio;
using AncientTempleDefense.Enemies;
using UnityEngine;

namespace AncientTempleDefense.Allies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class FriendlyDefender : MonoBehaviour
    {
        private static readonly List<FriendlyDefender> Active = new();

        [Header("Animasyonlar")]
        [SerializeField, InspectorName("Bekleme Animasyonu")]
        private string beklemeAnimasyonu = "Idle";
        [SerializeField, InspectorName("Hareket Animasyonu")]
        private string hareketAnimasyonu = "Run";
        [SerializeField, InspectorName("Birinci Saldiri Animasyonu")]
        private string birinciSaldiriAnimasyonu = "Attack1";
        [SerializeField, InspectorName("Ikinci Saldiri Animasyonu")]
        private string ikinciSaldiriAnimasyonu = "Attack2";
        [SerializeField, InspectorName("Hasar Alma Animasyonu")]
        private string hasarAlmaAnimasyonu = "Hit";
        [SerializeField, InspectorName("Olum Animasyonu")]
        private string olumAnimasyonu = "Death";

        [Header("Dost Savascisi")]
        [SerializeField, InspectorName("Azami Can"), Min(1)]
        private int azamiCan = 50;
        [SerializeField, InspectorName("Saldiri Hasari"), Min(1)]
        private int saldiriHasari = 1;
        [SerializeField, InspectorName("Hareket Hizi"), Min(0.1f)]
        private float hareketHizi = 2.35f;
        [SerializeField, InspectorName("Saldiri Menzili"), Min(0.1f)]
        private float saldiriMenzili = 1.45f;
        [SerializeField, InspectorName("Saldiri Bekleme Suresi"), Min(0.1f)]
        private float saldiriBeklemeSuresi = 0.9f;
        [SerializeField, InspectorName("Vurus Temas Orani"), Range(0.1f, 0.9f)]
        private float vurusTemasOrani = 0.48f;
        [SerializeField, InspectorName("Kaynak Varsayilan Yonu Sola")]
        private bool kaynakVarsayilanYonuSola;

        private SpriteRenderer _renderer;
        private Rigidbody2D _body;
        private Collider2D _collider;
        private AnimationStatePlayer _animations;
        private EnemyAudioController _audio;
        private EnemyCombatant _target;
        private float _nextTargetSearch;
        private float _nextAttackAt;
        private float _actionLockedUntil;
        private int _attackIndex;
        private bool _attacking;

        public static IReadOnlyList<FriendlyDefender> ActiveDefenders => Active;
        public static int AliveCount
        {
            get
            {
                int count = 0;
                foreach (FriendlyDefender defender in Active)
                {
                    if (defender != null && !defender.IsDead)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public event Action<FriendlyDefender> Died;
        public int MaximumHealth => azamiCan;
        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            Active.Clear();
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _animations = new AnimationStatePlayer(GetComponent<Animator>());
            _audio = GetComponent<EnemyAudioController>();
            CurrentHealth = azamiCan;
        }

        private void OnEnable()
        {
            CurrentHealth = azamiCan;
            _attacking = false;
            _actionLockedUntil = 0f;
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
            _animations.Play(beklemeAnimasyonu, true);
        }

        private void OnDisable()
        {
            Active.Remove(this);
            StopAllCoroutines();
        }

        private void FixedUpdate()
        {
            if (IsDead || _attacking || Time.time < _actionLockedUntil)
            {
                return;
            }

            RefreshTarget();
            if (_target == null)
            {
                _animations.Play(beklemeAnimasyonu);
                return;
            }

            float deltaX = _target.transform.position.x - transform.position.x;
            float distance = Mathf.Abs(deltaX);
            if (distance > 0.03f)
            {
                bool faceLeft = deltaX < 0f;
                _renderer.flipX = kaynakVarsayilanYonuSola ? !faceLeft : faceLeft;
            }

            if (distance <= saldiriMenzili)
            {
                if (Time.time >= _nextAttackAt)
                {
                    StartCoroutine(AttackSequence(_target));
                }
                else
                {
                    _animations.Play(beklemeAnimasyonu);
                }
                return;
            }

            Vector2 next = _body.position;
            next.x += Mathf.Sign(deltaX) * hareketHizi * Time.fixedDeltaTime;
            _body.MovePosition(next);
            _animations.Play(hareketAnimasyonu);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            if (IsDead)
            {
                StopAllCoroutines();
                StartCoroutine(DeathSequence());
                return;
            }

            float duration = _animations.Duration(hasarAlmaAnimasyonu, 0.25f);
            _actionLockedUntil = Time.time + duration;
            _animations.Play(hasarAlmaAnimasyonu, true);
            _audio?.PlayHit(duration);
        }

        private void RefreshTarget()
        {
            if (Time.time < _nextTargetSearch && _target != null && !_target.IsDead)
            {
                return;
            }

            _nextTargetSearch = Time.time + 0.15f;
            _target = null;
            float bestDistance = float.PositiveInfinity;
            IReadOnlyList<EnemyCombatant> enemies = EnemyCombatant.ActiveEnemies;
            for (int index = 0; index < enemies.Count; index++)
            {
                EnemyCombatant enemy = enemies[index];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                float distance = Mathf.Abs(enemy.transform.position.x - transform.position.x);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    _target = enemy;
                }
            }
        }

        private IEnumerator AttackSequence(EnemyCombatant target)
        {
            _attacking = true;
            string attackState = (_attackIndex++ & 1) == 0
                ? birinciSaldiriAnimasyonu
                : ikinciSaldiriAnimasyonu;
            if (string.IsNullOrEmpty(attackState))
            {
                attackState = birinciSaldiriAnimasyonu;
            }

            float duration = _animations.Duration(attackState, 0.55f);
            _animations.Play(attackState, true);
            _audio?.PlayAction(attackState, duration);
            _nextAttackAt = Time.time + saldiriBeklemeSuresi;
            float contactDelay = duration * vurusTemasOrani;
            yield return new WaitForSeconds(contactDelay);

            if (!IsDead && target != null && !target.IsDead
                && Mathf.Abs(target.transform.position.x - transform.position.x) <= saldiriMenzili * 1.15f)
            {
                target.TakeHit(saldiriHasari);
            }

            yield return new WaitForSeconds(Mathf.Max(0f, duration - contactDelay));
            _attacking = false;
            if (!IsDead)
            {
                _animations.Play(beklemeAnimasyonu, true);
            }
        }

        private IEnumerator DeathSequence()
        {
            _attacking = false;
            _body.linearVelocity = Vector2.zero;
            _collider.enabled = false;
            Died?.Invoke(this);
            float duration = _animations.Duration(olumAnimasyonu, 0.8f);
            _animations.Play(olumAnimasyonu, true);
            _audio?.PlayDeath(duration);
            yield return new WaitForSeconds(duration + 0.08f);
            Destroy(gameObject);
        }
    }
}
