using System.Collections;
using AncientTempleDefense.Allies;
using AncientTempleDefense.Player;
using UnityEngine;

namespace AncientTempleDefense.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyCombatant), typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public sealed class BossEnemyBrain : MonoBehaviour
    {
        [Header("Boss Kimliği")]
        [SerializeField, InspectorName("Boss Seviyesi"), Min(1)] private int bossSeviyesi = 1;

        [Header("Animasyon Durumları")]
        [SerializeField, InspectorName("Bekleme Animasyonu")] private string beklemeAnimasyonu = "Idle";
        [SerializeField, InspectorName("Hareket Animasyonu")] private string hareketAnimasyonu = "Walk";
        [SerializeField, InspectorName("Normal Saldırı Animasyonu")] private string normalSaldırıAnimasyonu = "Attack";
        [SerializeField, InspectorName("Ağır Saldırı Animasyonu")] private string ağırSaldırıAnimasyonu = "Cast";
        [SerializeField, InspectorName("Özel Hazırlık Animasyonu")] private string özelHazırlıkAnimasyonu = "";
        [SerializeField, InspectorName("Özel Saldırı Animasyonu")] private string özelSaldırıAnimasyonu = "Spell";

        [Header("Hareket ve Savaş")]
        [SerializeField, InspectorName("Hareket Hızı"), Min(0.1f)] private float hareketHızı = 1.35f;
        [SerializeField, InspectorName("Saldırı Menzili"), Min(0.1f)] private float saldırıMenzili = 1.8f;
        [SerializeField, InspectorName("Fark Etme Menzili"), Min(1f)] private float farkEtmeMenzili = 35f;
        [SerializeField, InspectorName("Saldırı Bekleme Süresi"), Min(0.1f)] private float saldırıBeklemeSüresi = 1.35f;
        [SerializeField, InspectorName("Temel Saldırı Hasarı"), Min(1)] private int temelSaldırıHasarı = 12;
        [SerializeField, InspectorName("Ağır Hasar Çarpanı"), Min(1f)] private float ağırHasarÇarpanı = 1.65f;
        [SerializeField, InspectorName("Özel Hasar Çarpanı"), Min(1f)] private float özelHasarÇarpanı = 2.4f;
        [SerializeField, InspectorName("Ağır Menzil Çarpanı"), Min(1f)] private float ağırMenzilÇarpanı = 1.35f;
        [SerializeField, InspectorName("Özel Menzil Çarpanı"), Min(1f)] private float özelMenzilÇarpanı = 2.2f;
        [SerializeField, InspectorName("Özel Saldırı Bekleme Süresi"), Min(0.5f)] private float özelSaldırıBeklemeSüresi = 5f;
        [SerializeField, InspectorName("Özel İçin Gereken Saldırı"), Min(1)] private int özelİçinGerekenSaldırı = 2;
        [SerializeField, InspectorName("Vuruş Temas Oranı"), Range(0.1f, 0.9f)] private float vuruşTemasOranı = 0.58f;
        [SerializeField, InspectorName("Kaynak Varsayilan Yonu Sola")] private bool kaynakVarsayilanYonuSola = true;

        [Header("Boss 1 Işınlanması")]
        [SerializeField, InspectorName("Işınlanma Sol Sınırı")] private float ışınlanmaSolSınırı = -14f;
        [SerializeField, InspectorName("Işınlanma Sağ Sınırı")] private float ışınlanmaSağSınırı = 15f;
        [SerializeField, InspectorName("En Az Işınlanma Mesafesi"), Min(0.1f)] private float enAzIşınlanmaMesafesi = 6f;

        [Header("İkinci Faz")]
        [SerializeField, InspectorName("İkinci Faz Can Eşiği"), Range(0.1f, 0.9f)] private float ikinciFazCanEşiği = 0.5f;
        [SerializeField, InspectorName("İkinci Faz Hareket Çarpanı"), Min(1f)] private float ikinciFazHareketÇarpanı = 1.35f;
        [SerializeField, InspectorName("İkinci Faz Vuruş Hızı Çarpanı"), Min(1f)] private float ikinciFazVuruşHızıÇarpanı = 1.45f;
        [SerializeField, InspectorName("İkinci Faz Hasar Çarpanı"), Min(1f)] private float ikinciFazHasarÇarpanı = 1.35f;
        [SerializeField, InspectorName("İkinci Faz Ölçek Çarpanı"), Min(1f)] private float ikinciFazÖlçekÇarpanı = 1.06f;
        [SerializeField, InspectorName("İkinci Faz Rengi")] private Color ikinciFazRengi = new(1f, 0.48f, 0.30f, 1f);

        private EnemyCombatant _combatant;
        private Rigidbody2D _body;
        private SpriteRenderer _spriteRenderer;
        private Transform _target;
        private Transform _playerTarget;
        private PlayerHealth _playerHealth;
        private float _nextAttackAt;
        private float _nextSpecialAt;
        private float _nextTargetRefresh;
        private bool _useHeavyAttack;
        private bool _attacking;
        private int _attacksSinceSpecial;
        private int _attackDamage;
        private float _attackSpeedMultiplier = 1f;
        private Vector3 _baseScale;

        public int BossTier => bossSeviyesi;
        public int AttackDamage => _attackDamage;
        public float HeavyDamageMultiplier => ağırHasarÇarpanı;
        public float SpecialDamageMultiplier => özelHasarÇarpanı;
        public float AttackSpeedMultiplier => _attackSpeedMultiplier;
        public string LastAttackState { get; private set; } = string.Empty;
        public int LastDamageDealt { get; private set; }
        public int SpecialAttackCount { get; private set; }
        public int LastSpecialDamage { get; private set; }
        public int TeleportCount { get; private set; }
        public bool IsPhaseTwo { get; private set; }
        public int PhaseTransitionCount { get; private set; }
        public float EffectiveAttackSpeedMultiplier => _attackSpeedMultiplier * (IsPhaseTwo ? ikinciFazVuruşHızıÇarpanı : 1f);
        public float EffectiveDamageMultiplier => IsPhaseTwo ? ikinciFazHasarÇarpanı : 1f;
        public bool IsAttacking => _attacking;

        private void Awake()
        {
            _combatant = GetComponent<EnemyCombatant>();
            _body = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _attackDamage = Mathf.Max(1, temelSaldırıHasarı);
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (_combatant != null)
            {
                _combatant.HealthChanged += OnBossHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (_combatant != null)
            {
                _combatant.HealthChanged -= OnBossHealthChanged;
            }

            StopAllCoroutines();
            _attacking = false;
        }

        public void Initialize(Transform target, int attackDamage, float attackSpeedMultiplier)
        {
            _target = target;
            _playerTarget = target;
            _playerHealth = target != null ? target.GetComponent<PlayerHealth>() : null;
            _nextTargetRefresh = 0f;
            _attackDamage = Mathf.Max(1, attackDamage);
            _attackSpeedMultiplier = Mathf.Max(0.1f, attackSpeedMultiplier);
            LastAttackState = string.Empty;
            LastDamageDealt = 0;
            SpecialAttackCount = 0;
            LastSpecialDamage = 0;
            TeleportCount = 0;
            IsPhaseTwo = false;
            PhaseTransitionCount = 0;
            transform.localScale = _baseScale;
            _combatant.SetPersistentTint(Color.white);
            _nextAttackAt = Time.time + 0.6f;
            _nextSpecialAt = Time.time + Mathf.Max(0.5f, özelSaldırıBeklemeSüresi * 0.5f);
        }

        private void FixedUpdate()
        {
            if (_combatant.IsDead)
            {
                return;
            }

            ResolveTarget();
            if (_target == null)
            {
                _combatant.PlayLoop(beklemeAnimasyonu);
                return;
            }

            float deltaX = _target.position.x - transform.position.x;
            float distance = Mathf.Abs(deltaX);
            if (distance > 0.05f)
            {
                _spriteRenderer.flipX = kaynakVarsayilanYonuSola ? deltaX > 0f : deltaX < 0f;
            }

            if (_attacking || _combatant.IsActionLocked)
            {
                return;
            }

            if (distance <= saldırıMenzili)
            {
                TryStartAttack();
                return;
            }

            if (distance <= farkEtmeMenzili)
            {
                float direction = Mathf.Sign(deltaX);
                Vector2 nextPosition = _body.position
                    + Vector2.right * (direction * hareketHızı * (IsPhaseTwo ? ikinciFazHareketÇarpanı : 1f) * Time.fixedDeltaTime);
                _body.MovePosition(nextPosition);
                _combatant.PlayLoop(hareketAnimasyonu);
                return;
            }

            _combatant.PlayLoop(beklemeAnimasyonu);
        }

        private void ResolveTarget()
        {
            if (Time.time < _nextTargetRefresh && IsLivingTarget(_target))
            {
                return;
            }

            _nextTargetRefresh = Time.time + 0.20f;
            if (_playerTarget == null)
            {
                BlackKnightPlayerController player = FindFirstObjectByType<BlackKnightPlayerController>();
                _playerTarget = player != null ? player.transform : null;
            }

            PlayerHealth playerHealth = _playerTarget != null
                ? _playerTarget.GetComponent<PlayerHealth>()
                : null;
            Transform bestTarget = playerHealth == null || !playerHealth.IsDead
                ? _playerTarget
                : null;
            float bestDistance = bestTarget != null
                ? Mathf.Abs(bestTarget.position.x - transform.position.x)
                : float.PositiveInfinity;

            var defenders = FriendlyDefender.ActiveDefenders;
            for (int index = 0; index < defenders.Count; index++)
            {
                FriendlyDefender defender = defenders[index];
                if (defender == null || defender.IsDead)
                {
                    continue;
                }

                float distance = Mathf.Abs(defender.transform.position.x - transform.position.x);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = defender.transform;
                }
            }

            _target = bestTarget;
            _playerHealth = _target != null ? _target.GetComponent<PlayerHealth>() : null;
        }

        private static bool IsLivingTarget(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            PlayerHealth player = target.GetComponent<PlayerHealth>();
            if (player != null)
            {
                return !player.IsDead;
            }

            FriendlyDefender defender = target.GetComponent<FriendlyDefender>();
            return defender != null && !defender.IsDead;
        }

        private void TryStartAttack()
        {
            if (Time.time < _nextAttackAt)
            {
                _combatant.PlayLoop(beklemeAnimasyonu);
                return;
            }

            int attacksNeededForSpecial = Mathf.Max(1, özelİçinGerekenSaldırı - (IsPhaseTwo ? 1 : 0));
            bool specialReady = _attacksSinceSpecial >= attacksNeededForSpecial
                && Time.time >= _nextSpecialAt
                && !string.IsNullOrEmpty(özelSaldırıAnimasyonu);
            if (specialReady)
            {
                _attacksSinceSpecial = 0;
                _nextSpecialAt = Time.time + özelSaldırıBeklemeSüresi / EffectiveAttackSpeedMultiplier;
                StartCoroutine(AttackSequence(
                    özelSaldırıAnimasyonu,
                    özelHazırlıkAnimasyonu,
                    özelHasarÇarpanı,
                    özelMenzilÇarpanı,
                    true));
                return;
            }

            bool heavy = _useHeavyAttack && !string.IsNullOrEmpty(ağırSaldırıAnimasyonu);
            _useHeavyAttack = !_useHeavyAttack;
            _attacksSinceSpecial++;
            StartCoroutine(AttackSequence(
                heavy ? ağırSaldırıAnimasyonu : normalSaldırıAnimasyonu,
                string.Empty,
                heavy ? ağırHasarÇarpanı : 1f,
                heavy ? ağırMenzilÇarpanı : 1f,
                false));
        }

        private IEnumerator AttackSequence(
            string attackState,
            string preparationState,
            float damageMultiplier,
            float rangeMultiplier,
            bool specialAttack)
        {
            _attacking = true;

            if (!string.IsNullOrEmpty(preparationState))
            {
                float preparationDuration = _combatant.AnimationDuration(preparationState, 0.8f);
                if (_combatant.TryPlayAction(preparationState, preparationDuration))
                {
                    yield return new WaitForSeconds(preparationDuration);
                }
            }

            float duration = _combatant.AnimationDuration(attackState, 0.8f);
            if (!_combatant.TryPlayAction(attackState, duration))
            {
                _attacking = false;
                yield break;
            }

            LastAttackState = attackState;
            _nextAttackAt = Time.time + saldırıBeklemeSüresi / EffectiveAttackSpeedMultiplier;
            float contactDelay = duration * vuruşTemasOranı;
            yield return new WaitForSeconds(contactDelay);

            if (_target != null
                && Mathf.Abs(_target.position.x - transform.position.x) <= saldırıMenzili * rangeMultiplier)
            {
                int damage = Mathf.Max(1, Mathf.RoundToInt(_attackDamage * damageMultiplier * EffectiveDamageMultiplier));
                if (_playerHealth != null)
                {
                    _playerHealth.TakeDamage(damage);
                }
                else
                {
                    _target.GetComponent<FriendlyDefender>()?.TakeDamage(damage);
                }
                LastDamageDealt = damage;

                if (specialAttack)
                {
                    SpecialAttackCount++;
                    LastSpecialDamage = damage;
                }
            }

            if (specialAttack)
            {
                TeleportBossOne();
            }

            yield return new WaitForSeconds(Mathf.Max(0f, duration - contactDelay));
            _attacking = false;
        }

        private void TeleportBossOne()
        {
            if (bossSeviyesi != 1)
            {
                return;
            }

            float left = Mathf.Min(ışınlanmaSolSınırı, ışınlanmaSağSınırı);
            float right = Mathf.Max(ışınlanmaSolSınırı, ışınlanmaSağSınırı);
            Vector2 currentPosition = _body != null ? _body.position : (Vector2)transform.position;
            float destinationX;

            if (_target != null)
            {
                destinationX = Mathf.Abs(_target.position.x - left) >= Mathf.Abs(_target.position.x - right)
                    ? left
                    : right;
            }
            else
            {
                destinationX = currentPosition.x <= (left + right) * 0.5f ? right : left;
            }

            if (Mathf.Abs(destinationX - currentPosition.x) < enAzIşınlanmaMesafesi)
            {
                destinationX = Mathf.Approximately(destinationX, left) ? right : left;
            }

            Vector2 destination = new(destinationX, currentPosition.y);
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
                _body.position = destination;
            }
            else
            {
                transform.position = new Vector3(destination.x, destination.y, transform.position.z);
            }

            TeleportCount++;
        }

        private void OnBossHealthChanged(EnemyCombatant combatant, int currentHealth, int maximumHealth)
        {
            if (IsPhaseTwo || currentHealth <= 0 || maximumHealth <= 0)
            {
                return;
            }

            if ((float)currentHealth / maximumHealth > ikinciFazCanEşiği)
            {
                return;
            }

            IsPhaseTwo = true;
            PhaseTransitionCount++;
            transform.localScale = _baseScale * ikinciFazÖlçekÇarpanı;
            _combatant.SetPersistentTint(Color.white);
            _attacksSinceSpecial = Mathf.Max(_attacksSinceSpecial, Mathf.Max(1, özelİçinGerekenSaldırı - 1));
            _nextAttackAt = Mathf.Min(_nextAttackAt, Time.time + 0.25f);
            _nextSpecialAt = Mathf.Min(_nextSpecialAt, Time.time + 0.6f);
        }
    }
}