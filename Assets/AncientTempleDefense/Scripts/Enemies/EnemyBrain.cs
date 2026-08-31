using AncientTempleDefense.Allies;
using AncientTempleDefense.Player;
using AncientTempleDefense.Temple;
using AncientTempleDefense.Vfx;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientTempleDefense.Enemies
{
[DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyCombatant), typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public sealed class EnemyBrain : MonoBehaviour
    {
        [Header("Animasyon Durumlar\u0131")]
        [FormerlySerializedAs("idleState")]
        [SerializeField, InspectorName("Bekleme Animasyonu")] private string beklemeAnimasyonu = "Idle";
        [FormerlySerializedAs("moveState")]
        [SerializeField, InspectorName("Hareket Animasyonu")] private string hareketAnimasyonu = "Move";
        [FormerlySerializedAs("attackOneState")]
        [SerializeField, InspectorName("Birinci Sald\u0131r\u0131")] private string birinciSaldırıAnimasyonu = "Attack1";
        [FormerlySerializedAs("attackTwoState")]
        [SerializeField, InspectorName("\u0130kinci Sald\u0131r\u0131")] private string ikinciSaldırıAnimasyonu = "Attack2";
        [SerializeField, InspectorName("Üçüncü Saldırı")] private string üçüncüSaldırıAnimasyonu = "";
        [FormerlySerializedAs("defenseState")]
        [SerializeField, InspectorName("Savunma Animasyonu")] private string savunmaAnimasyonu = "";

        [Header("Hareket ve Sava\u015f")]
        [FormerlySerializedAs("moveSpeed")]
        [SerializeField, InspectorName("Hareket H\u0131z\u0131")] private float hareketHızı = 1.8f;
        [FormerlySerializedAs("attackRange")]
        [SerializeField, InspectorName("Sald\u0131r\u0131 Menzili")] private float saldırıMenzili = 1.45f;
        [FormerlySerializedAs("noticeRange")]
        [SerializeField, InspectorName("Fark Etme Menzili")] private float farkEtmeMenzili = 30f;
        [FormerlySerializedAs("attackCooldown")]
        [SerializeField, InspectorName("Sald\u0131r\u0131 Bekleme S\u00fcresi")] private float saldırıBeklemeSüresi = 1.2f;
        [FormerlySerializedAs("baseAttackDamage")]
        [SerializeField, InspectorName("Temel Sald\u0131r\u0131 Hasar\u0131"), Min(1)] private int temelSaldırıHasarı = 8;
        [SerializeField, InspectorName("Kaynak Varsayılan Yönü Sola")] private bool kaynakVarsayilanYonuSola;

        private EnemyCombatant _combatant;
        private EnemyRoleProfile _role;
        private EnemyVfxController _effects;
        private Rigidbody2D _body;
private SpriteRenderer _spriteRenderer;
        private Transform _target;
        private Transform _playerTarget;
        private PlayerHealth _playerHealth;
        private TempleHealth _templeTarget;
        private float _nextTargetRefresh;
        private float _nextAttackAt;
        private float _nextDefenseCheck;
        private int _nextAttackIndex;
        private int _attackDamage;
        private float _attackSpeedMultiplier = 1f;

        public int AttackDamage => _attackDamage;
        public float AttackSpeedMultiplier => _attackSpeedMultiplier;
        public Transform CurrentTarget => _target;
        public EnemyTargetMode TargetMode => _role != null ? _role.TargetMode : EnemyTargetMode.NearestThreat;

        private void Awake()
        {
            _combatant = GetComponent<EnemyCombatant>();
            _body = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _role = GetComponent<EnemyRoleProfile>();
            _effects = GetComponent<EnemyVfxController>();
            if (_role != null)
            {
                hareketHızı *= _role.MovementSpeedMultiplier;
            }
            _attackDamage = Mathf.Max(1, temelSaldırıHasarı);
        }

        public void Initialize(Transform target)
        {
            Initialize(target, temelSaldırıHasarı, 1f);
        }

        public void Initialize(Transform target, int attackDamage, float attackSpeedMultiplier)
        {
            _target = target;
            _attackDamage = Mathf.Max(1, attackDamage);
            _playerTarget = target;
            _playerHealth = target != null ? target.GetComponent<PlayerHealth>() : null;
            _attackSpeedMultiplier = Mathf.Max(0.1f, attackSpeedMultiplier);
        }

        private void FixedUpdate()
        {
            if (_combatant.IsDead)
            {
                return;
            }

            RefreshTarget();
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

            if (_combatant.IsActionLocked)
            {
                return;
            }

            if (distance <= saldırıMenzili)
            {
                TryAttackOrDefend();
                return;
            }

            if (distance <= farkEtmeMenzili)
            {
                float direction = Mathf.Sign(deltaX);
                Vector2 nextPosition = _body.position + Vector2.right * (direction * hareketHızı * Time.fixedDeltaTime);
                _body.MovePosition(nextPosition);
                _combatant.PlayLoop(hareketAnimasyonu);
                return;
            }

            _combatant.PlayLoop(beklemeAnimasyonu);
        }

        private void RefreshTarget()
        {
            if (Time.time < _nextTargetRefresh)
            {
                return;
            }

            _nextTargetRefresh = Time.time + 0.20f;
            if (_role != null && _role.TargetMode != EnemyTargetMode.NearestThreat)
            {
                RefreshRoleTargets();
                _target = ResolveRoleTarget();
                return;
            }
            if (_playerTarget == null)
            {
                BlackKnightPlayerController player = FindFirstObjectByType<BlackKnightPlayerController>();
                _playerTarget = player != null ? player.transform : null;
                _playerHealth = player != null ? player.GetComponent<PlayerHealth>() : null;
            }

            if (_templeTarget == null)
            {
                _templeTarget = FindFirstObjectByType<TempleHealth>();
            }

            Transform bestTarget = _playerHealth == null || !_playerHealth.IsDead ? _playerTarget : null;
            float bestDistance = bestTarget != null
                ? Mathf.Abs(bestTarget.position.x - transform.position.x)
                : float.PositiveInfinity;

            if (_templeTarget != null && !_templeTarget.IsDestroyed)
            {
                float templeDistance = Mathf.Abs(_templeTarget.transform.position.x - transform.position.x);
                if (templeDistance < bestDistance)
                {
                    bestDistance = templeDistance;
                    bestTarget = _templeTarget.transform;
                }
            }

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
        }

        private void RefreshRoleTargets()
        {
            if (_playerTarget == null)
            {
                BlackKnightPlayerController player = FindFirstObjectByType<BlackKnightPlayerController>();
                _playerTarget = player != null ? player.transform : null;
                _playerHealth = player != null ? player.GetComponent<PlayerHealth>() : null;
            }

            if (_templeTarget == null)
            {
                _templeTarget = FindFirstObjectByType<TempleHealth>();
            }
        }

        private Transform ResolveRoleTarget()
        {
            Transform livingPlayer = _playerTarget != null && (_playerHealth == null || !_playerHealth.IsDead)
                ? _playerTarget
                : null;
            if (_role.TargetMode == EnemyTargetMode.PlayerOnly)
            {
                return livingPlayer;
            }

            if (_role.TargetMode == EnemyTargetMode.TempleOnly)
            {
                return _templeTarget != null && !_templeTarget.IsDestroyed
                    ? _templeTarget.transform
                    : livingPlayer;
            }

            Transform bestTarget = livingPlayer;
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

            return bestTarget;
        }

        private void TryAttackOrDefend()
        {
            if (Time.time < _nextAttackAt)
            {
                if (!string.IsNullOrEmpty(savunmaAnimasyonu) && Time.time >= _nextDefenseCheck)
                {
                    _nextDefenseCheck = Time.time + 1.4f;
                    if (Random.value < 0.35f)
                    {
                        _combatant.TryPlayAction(savunmaAnimasyonu, 0.45f);
                        return;
                    }
                }

                _combatant.PlayLoop(beklemeAnimasyonu);
                return;
            }

            int attackCount = string.IsNullOrEmpty(üçüncüSaldırıAnimasyonu) ? 2 : 3;
            string attackState = _nextAttackIndex switch
            {
                0 => birinciSaldırıAnimasyonu,
                1 => ikinciSaldırıAnimasyonu,
                _ => üçüncüSaldırıAnimasyonu
            };
            _nextAttackIndex = (_nextAttackIndex + 1) % attackCount;
            if (_combatant.TryPlayAction(attackState, 0.7f))
            {
                _nextAttackAt = Time.time + saldırıBeklemeSüresi / _attackSpeedMultiplier;
                PlayerHealth health = _target.GetComponent<PlayerHealth>();
                health?.TakeDamage(_attackDamage);
                if (health == null)
                {
                    FriendlyDefender defender = _target.GetComponent<FriendlyDefender>();
                    if (defender != null)
                    {
                        defender.TakeDamage(_attackDamage);
                    }
                    else
                    {
                        _target.GetComponent<TempleHealth>()?.TakeDamage(_attackDamage);
                    }
                }
                _effects?.PlayAttackImpact(_target.position);
            }        }
    }
}
