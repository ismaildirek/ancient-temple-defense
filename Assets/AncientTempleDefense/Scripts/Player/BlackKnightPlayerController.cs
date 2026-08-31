using System.Collections;
using System.Collections.Generic;
using AncientTempleDefense.Animation;
using AncientTempleDefense.Audio;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Progression;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

namespace AncientTempleDefense.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class BlackKnightPlayerController : MonoBehaviour
    {
        private static readonly string[] LightAttacks =
        {
            "BK_attack_1", "BK_attack_2", "BK_attack_3", "BK_attack_4"
        };

        private static readonly string[] HeavyAttacks =
        {
            "BK_heavy_attack_1", "BK_heavy_attack_2", "BK_heavy_attack_3"
        };

        [Header("Hareket")]
        [FormerlySerializedAs("moveSpeed")]
        [SerializeField, InspectorName("Hareket H\u0131z\u0131")] private float hareketHızı = 4.5f;
        [FormerlySerializedAs("jumpImpulse")]
        [SerializeField, InspectorName("Z\u0131plama Kuvveti")] private float zıplamaKuvveti = 8f;
        [FormerlySerializedAs("rollSpeed")]
        [SerializeField, InspectorName("Takla H\u0131z\u0131")] private float taklaHızı = 7.5f;
        [FormerlySerializedAs("horizontalBounds")]
        [SerializeField, InspectorName("Yatay S\u0131n\u0131rlar")] private Vector2 yataySınırlar = new(-17.5f, 18.5f);

        [Header("Sava\u015f")]
        [FormerlySerializedAs("attackBoxSize")]
        [SerializeField, InspectorName("Sald\u0131r\u0131 Alan\u0131")] private Vector2 saldırıAlanı = new(2.2f, 2.2f);
        [FormerlySerializedAs("attackOffset")]
        [SerializeField, InspectorName("Sald\u0131r\u0131 Uzakl\u0131\u011f\u0131")] private float saldırıUzaklığı = 1.15f;
        [SerializeField, InspectorName("Küçük Düşman Yakalama Payı"), Min(0f)] private float küçükDüşmanYakalamaPayı = 0.35f;
        [FormerlySerializedAs("lightAttackDamage")]
        [SerializeField, InspectorName("Hafif Sald\u0131r\u0131 Hasar\u0131"), Min(1)] private int hafifSaldırıHasarı = 1;
        [FormerlySerializedAs("heavyAttackDamage")]
        [SerializeField, InspectorName("A\u011f\u0131r Sald\u0131r\u0131 Hasar\u0131"), Min(1)] private int ağırSaldırıHasarı = 2;
        [FormerlySerializedAs("ultimateAttackDamage")]
        [SerializeField, InspectorName("Ulti Hasar\u0131"), Min(1)] private int ultiHasarı = 3;
        [FormerlySerializedAs("ultimateRangeMultiplier")]
        [SerializeField, InspectorName("Ulti Menzil \u00c7arpan\u0131")] private float ultiMenzilÇarpanı = 2.8f;
        [FormerlySerializedAs("ultimateCooldown")]
        [SerializeField, InspectorName("Ulti Bekleme S\u00fcresi")] private float ultiBeklemeSüresi = 10f;
        [FormerlySerializedAs("bonusDuration")]
        [SerializeField, InspectorName("Bonus S\u00fcresi")] private float bonusSüresi = 6f;
        [FormerlySerializedAs("bonusMoveMultiplier")]
        [SerializeField, InspectorName("Bonus Hareket \u00c7arpan\u0131")] private float bonusHareketÇarpanı = 1.35f;

        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[6];
        private readonly Collider2D[] _attackHits = new Collider2D[32];
        private readonly HashSet<EnemyCombatant> _hitBuffer = new();

        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _body;
        private Collider2D _collider;
        private AnimationStatePlayer _animations;
        private BlackKnightSwordAudio _swordAudio;
        private ContactFilter2D _groundFilter;
        private ContactFilter2D _attackFilter;
        private PlayerHealth _playerHealth;

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _rollAction;
        private InputAction _lightAttackAction;
        private InputAction _heavyAttackAction;
        private InputAction _parryAction;
        private InputAction _ultimateAction;
        private InputAction _weaponToggleAction;

        private float _horizontalInput;
        private float _bonusEndsAt;
        private float _ultimateReadyAt;
        private int _facing = 1;
        private int _lightAttackIndex;
        private int _heavyAttackIndex;
        private bool _grounded;
        private bool _wasGrounded;
        private bool _busy;
        private bool _attackInProgress;
        private bool _ultimateInProgress;
        private bool _weaponDrawn = true;

        public bool IsDefending { get; private set; }
        public bool IsAttacking => _attackInProgress;
        public bool IsUltimateInProgress => _ultimateInProgress;
        public int LightAttackDamage => hafifSaldırıHasarı;
        public int HeavyAttackDamage => ağırSaldırıHasarı;
        public int UltimateAttackDamage => ultiHasarı;
        public float MoveSpeed => hareketHızı;
        public float SmallEnemyAssistMargin => küçükDüşmanYakalamaPayı;
        public float AttackReach => saldırıUzaklığı;
        public float UltimateCooldown => ultiBeklemeSüresi;

        public void ApplyUpgrade(PlayerUpgradeType type)
        {
            switch (type)
            {
                case PlayerUpgradeType.LightDamage:
                    hafifSaldırıHasarı++;
                    break;
                case PlayerUpgradeType.HeavyDamage:
                    ağırSaldırıHasarı++;
                    break;
                case PlayerUpgradeType.UltimateDamage:
                    ultiHasarı += 2;
                    break;
                case PlayerUpgradeType.MoveSpeed:
                    hareketHızı *= 1.12f;
                    break;
                case PlayerUpgradeType.AttackReach:
                    saldırıUzaklığı *= 1.12f;
                    saldırıAlanı.x *= 1.12f;
                    break;
                case PlayerUpgradeType.UltimateCooldown:
                    ultiBeklemeSüresi = Mathf.Max(2.5f, ultiBeklemeSüresi * 0.85f);
                    break;
                case PlayerUpgradeType.MaximumHealth:
                    _playerHealth?.IncreaseMaximumHealth(25);
                    break;
                case PlayerUpgradeType.Armor:
                    _playerHealth?.IncreaseArmor(8);
                    break;
            }
        }

        private string IdleState => _weaponDrawn ? "BK_weapon_idle" : "BK_unarmed_idle";
        private string RunState => _weaponDrawn ? "BK_weapon_run" : "BK_unarmed_run";
        private string JumpState => _weaponDrawn ? "BK_weapon_jump" : "BK_unarmed_jump";
        private string MidairState => _weaponDrawn ? "BK_weapon_midair" : "BK_unarmed_midair";
        private string LandState => _weaponDrawn ? "BK_weapon_land" : "BK_unarmed_land";
        private string RollState => _weaponDrawn ? "BK_weapon_roll" : "BK_unarmed_roll";
        private string FallLightState => _weaponDrawn ? "BK_weapon_fall_light" : "BK_unarmed_fall_light";
        private string FallHeavyState => _weaponDrawn ? "BK_weapon_fall_heavy" : "BK_unarmed_fall_heavy";

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _animations = new AnimationStatePlayer(_animator);
            _swordAudio = GetComponent<BlackKnightSwordAudio>();
            _playerHealth = GetComponent<PlayerHealth>();
            _groundFilter = ContactFilter2D.noFilter;
            _groundFilter.useTriggers = false;
            _attackFilter = ContactFilter2D.noFilter;
            _attackFilter.useTriggers = true;
            CreateInputActions();
        }

        private void OnEnable()
        {
            SetInputActionsEnabled(true);
            _animations.Play(IdleState, true);
        }

        private void OnDisable()
        {
            SetInputActionsEnabled(false);
            StopAllCoroutines();
            _busy = false;
            _attackInProgress = false;
            _ultimateInProgress = false;
            IsDefending = false;
        }

        private void OnDestroy()
        {
            _moveAction?.Dispose();
            _jumpAction?.Dispose();
            _rollAction?.Dispose();
            _lightAttackAction?.Dispose();
            _heavyAttackAction?.Dispose();
            _parryAction?.Dispose();
            _ultimateAction?.Dispose();
            _weaponToggleAction?.Dispose();
        }

        private void Update()
        {
            UpdateBonusPresentation();

            float requestedHorizontal = _moveAction.ReadValue<float>();
            bool jumpPressed = _jumpAction.WasPressedThisFrame();
            bool rollPressed = _rollAction.WasPressedThisFrame();
            bool movementRequested = Mathf.Abs(requestedHorizontal) > 0.01f || jumpPressed || rollPressed;

            if (_busy && _attackInProgress && !_ultimateInProgress && movementRequested)
            {
                CancelAttackForMovement();
            }

            if (_busy)
            {
                _horizontalInput = 0f;
                return;
            }

            _horizontalInput = requestedHorizontal;
            if (Mathf.Abs(_horizontalInput) > 0.01f)
            {
                _facing = _horizontalInput > 0f ? 1 : -1;
                _spriteRenderer.flipX = _facing < 0;
            }

            if (_grounded && jumpPressed)
            {
                StartCoroutine(JumpSequence());
                return;
            }

            if (_grounded && rollPressed)
            {
                StartCoroutine(RollSequence());
                return;
            }

            if (Mathf.Abs(_horizontalInput) > 0.01f)
            {
                UpdateLocomotionAnimation();
                return;
            }

            if (_lightAttackAction.WasPressedThisFrame())
            {
                StartCoroutine(AttackSequence(
                    LightAttacks[_lightAttackIndex++ % LightAttacks.Length],
                    1f,
                    LightAttackDamage));
                return;
            }

            if (_heavyAttackAction.WasPressedThisFrame())
            {
                StartCoroutine(AttackSequence(
                    HeavyAttacks[_heavyAttackIndex++ % HeavyAttacks.Length],
                    1.2f,
                    HeavyAttackDamage));
                return;
            }

            if (_parryAction.WasPressedThisFrame())
            {
                StartCoroutine(ParrySequence());
                return;
            }

            if (_ultimateAction.WasPressedThisFrame() && Time.time >= _ultimateReadyAt)
            {
                StartCoroutine(UltimateSequence());
                return;
            }

            if (_weaponToggleAction.WasPressedThisFrame())
            {
                StartCoroutine(ToggleWeaponSequence());
                return;
            }

            // Hareket tusu birakildigi karede kosu klibini hemen kes.
            UpdateLocomotionAnimation();
        }

        private void CancelAttackForMovement()
        {
            StopAllCoroutines();
            _busy = false;
            _attackInProgress = false;
            IsDefending = false;
            _swordAudio?.StopPlayback();
        }

        private void FixedUpdate()
        {
            _wasGrounded = _grounded;
            _grounded = _collider.Cast(Vector2.down, _groundFilter, _groundHits, 0.08f) > 0;

            Vector2 velocity = _body.linearVelocity;
            float speedMultiplier = Time.time < _bonusEndsAt ? bonusHareketÇarpanı : 1f;
            velocity.x = _ultimateInProgress
                ? 0f
                : (_busy ? velocity.x : _horizontalInput * hareketHızı * speedMultiplier);
            _body.linearVelocity = velocity;

            Vector2 position = _body.position;
            position.x = Mathf.Clamp(position.x, yataySınırlar.x, yataySınırlar.y);
            _body.position = position;

            if (!_wasGrounded && _grounded && !_busy)
            {
                StartCoroutine(LandingSequence());
            }
        }

        private void CreateInputActions()
        {
            _moveAction = new InputAction("HorizontalMove", InputActionType.Value);
            _moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftArrow")
                .With("Positive", "<Keyboard>/rightArrow");

            _jumpAction = CreateButtonAction("Jump", "<Keyboard>/w", "<Keyboard>/upArrow");
            _rollAction = CreateButtonAction("Roll", "<Keyboard>/s", "<Keyboard>/downArrow");
            _lightAttackAction = CreateButtonAction("LightAttack", "<Keyboard>/1", "<Keyboard>/numpad1");
            _heavyAttackAction = CreateButtonAction("HeavyAttack", "<Keyboard>/2", "<Keyboard>/numpad2");
            _parryAction = CreateButtonAction("Parry", "<Keyboard>/3", "<Keyboard>/numpad3");
            _ultimateAction = CreateButtonAction("Ultimate", "<Keyboard>/4", "<Keyboard>/numpad4");
            _weaponToggleAction = CreateButtonAction("WeaponToggle", "<Keyboard>/q");
        }

        private static InputAction CreateButtonAction(string name, params string[] bindings)
        {
            InputAction action = new(name, InputActionType.Button);
            foreach (string binding in bindings)
            {
                action.AddBinding(binding);
            }

            return action;
        }

        private void SetInputActionsEnabled(bool enabled)
        {
            InputAction[] actions =
            {
                _moveAction, _jumpAction, _rollAction, _lightAttackAction,
                _heavyAttackAction, _parryAction, _ultimateAction, _weaponToggleAction
            };

            foreach (InputAction action in actions)
            {
                if (enabled)
                {
                    action.Enable();
                }
                else
                {
                    action.Disable();
                }
            }
        }

        private IEnumerator AttackSequence(string attackState, float rangeMultiplier, int damage)
        {
            _busy = true;
            _attackInProgress = true;
            yield return EnsureWeaponDrawn();

            float duration = _animations.Duration(attackState, 0.65f);
            _animations.Play(attackState, true);
            _swordAudio?.PlayAttack(rangeMultiplier > 1f, duration, 0.42f);
            yield return new WaitForSeconds(duration * 0.42f);
            DealDamage(rangeMultiplier, damage);
            yield return new WaitForSeconds(duration * 0.58f);

            _busy = false;
            UpdateLocomotionAnimation();
        }

        private IEnumerator ParrySequence()
        {
            _busy = true;
            yield return EnsureWeaponDrawn();

            float parryStartDuration = _animations.Duration("BK_parry_start");
            _animations.Play("BK_parry_start", true);
            _swordAudio?.PlayParry(parryStartDuration);
            yield return new WaitForSeconds(parryStartDuration);

            IsDefending = true;
            _animations.Play("BK_parry", true);
            yield return new WaitForSeconds(_animations.Duration("BK_parry", 1f));
            IsDefending = false;

            _animations.Play("BK_parry_miss", true);
            yield return new WaitForSeconds(_animations.Duration("BK_parry_miss", 0.2f));
            _busy = false;
            UpdateLocomotionAnimation();
        }

        private IEnumerator UltimateSequence()
        {
            _busy = true;
            _attackInProgress = true;
            _ultimateInProgress = true;
            _horizontalInput = 0f;
            Vector2 stoppedVelocity = _body.linearVelocity;
            stoppedVelocity.x = 0f;
            _body.linearVelocity = stoppedVelocity;
            _ultimateReadyAt = Time.time + ultiBeklemeSüresi;
            yield return EnsureWeaponDrawn();

            _animations.Play("BK_weapon_buff", true);
            yield return new WaitForSeconds(_animations.Duration("BK_weapon_buff", 1f));
            _bonusEndsAt = Time.time + bonusSüresi;

            float artDuration = _animations.Duration("BK_weapon_art", 2.3f);
            _animations.Play("BK_weapon_art", true);
            _swordAudio?.PlayUltimate(artDuration, 0.58f);
            yield return new WaitForSeconds(artDuration * 0.58f);
            DealDamage(ultiMenzilÇarpanı, ultiHasarı);
            yield return new WaitForSeconds(artDuration * 0.42f);

            _ultimateInProgress = false;
            _busy = false;
            UpdateLocomotionAnimation();
        }

        private IEnumerator ToggleWeaponSequence()
        {
            _busy = true;
            string state = _weaponDrawn ? "BK_weapon_off" : "BK_weapon_on";
            float duration = _animations.Duration(state, 1f);
            _animations.Play(state, true);
            _swordAudio?.PlayWeaponToggle(!_weaponDrawn, duration);
            yield return new WaitForSeconds(duration);
            _weaponDrawn = !_weaponDrawn;
            _busy = false;
            UpdateLocomotionAnimation();
        }

        private IEnumerator EnsureWeaponDrawn()
        {
            if (_weaponDrawn)
            {
                yield break;
            }

            float duration = _animations.Duration("BK_weapon_on", 1.2f);
            _animations.Play("BK_weapon_on", true);
            _swordAudio?.PlayWeaponToggle(true, duration);
            yield return new WaitForSeconds(duration);
            _weaponDrawn = true;
        }

        private IEnumerator JumpSequence()
        {
            _busy = true;
            _animations.Play(JumpState, true);
            Vector2 velocity = _body.linearVelocity;
            velocity.y = zıplamaKuvveti;
            _body.linearVelocity = velocity;
            _grounded = false;
            yield return new WaitForSeconds(_animations.Duration(JumpState));
            _busy = false;
            _animations.Play(MidairState, true);
        }

        private IEnumerator RollSequence()
        {
            _busy = true;
            _animations.Play(RollState, true);
            float duration = _animations.Duration(RollState, 0.8f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Vector2 velocity = _body.linearVelocity;
                velocity.x = _facing * taklaHızı;
                _body.linearVelocity = velocity;
                elapsed += Time.deltaTime;
                yield return null;
            }

            _busy = false;
            UpdateLocomotionAnimation();
        }

        private IEnumerator LandingSequence()
        {
            _busy = true;
            _animations.Play(LandState, true);
            yield return new WaitForSeconds(_animations.Duration(LandState));
            _busy = false;
            UpdateLocomotionAnimation();
        }

        private void DealDamage(float rangeMultiplier, int damage)
        {
            Vector2 center = (Vector2)transform.position + Vector2.right * (_facing * saldırıUzaklığı * rangeMultiplier);
            Vector2 size = new(saldırıAlanı.x * rangeMultiplier, saldırıAlanı.y);
            int hitCount = Physics2D.OverlapBox(center, size, 0f, _attackFilter, _attackHits);
            _hitBuffer.Clear();

            for (int index = 0; index < hitCount; index++)
            {
                Collider2D overlap = _attackHits[index];
                _attackHits[index] = null;
                if (overlap == null)
                {
                    continue;
                }

                EnemyCombatant enemy = overlap.GetComponentInParent<EnemyCombatant>();
                if (enemy != null && _hitBuffer.Add(enemy))
                {
                    enemy.TakeHit(damage);
                }
            }

            IReadOnlyList<EnemyCombatant> activeEnemies = EnemyCombatant.ActiveEnemies;
            for (int index = 0; index < activeEnemies.Count; index++)
            {
                EnemyCombatant enemy = activeEnemies[index];
                if (enemy == null
                    || enemy.IsDead
                    || _hitBuffer.Contains(enemy)
                    || !IsInsideDirectedAttackSweep(enemy, rangeMultiplier))
                {
                    continue;
                }

                _hitBuffer.Add(enemy);
                enemy.TakeHit(damage);
            }
        }

        private bool IsInsideDirectedAttackSweep(EnemyCombatant enemy, float rangeMultiplier)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider == null || !enemyCollider.enabled)
            {
                return false;
            }

            Bounds bounds = enemyCollider.bounds;
            Vector2 origin = transform.position;
            float forwardDistance = (bounds.center.x - origin.x) * _facing;
            float horizontalHalfSize = saldırıAlanı.x * rangeMultiplier * 0.5f;
            float forwardReach = saldırıUzaklığı * rangeMultiplier
                + horizontalHalfSize
                + küçükDüşmanYakalamaPayı;
            float rearAllowance = Mathf.Min(0.22f, horizontalHalfSize * 0.18f) + bounds.extents.x;
            if (forwardDistance < -rearAllowance || forwardDistance > forwardReach + bounds.extents.x)
            {
                return false;
            }

            float verticalDistance = Mathf.Abs(bounds.center.y - origin.y);
            float verticalReach = saldırıAlanı.y * 0.5f
                + küçükDüşmanYakalamaPayı
                + bounds.extents.y;
            return verticalDistance <= verticalReach;
        }

        private void UpdateLocomotionAnimation()
        {
            if (_busy)
            {
                return;
            }

            _attackInProgress = false;

            if (!_grounded)
            {
                if (_body.linearVelocity.y < -5.5f)
                {
                    _animations.Play(FallHeavyState);
                }
                else if (_body.linearVelocity.y < -0.25f)
                {
                    _animations.Play(FallLightState);
                }
                else
                {
                    _animations.Play(MidairState);
                }

                return;
            }

            _animations.Play(Mathf.Abs(_horizontalInput) > 0.05f ? RunState : IdleState);
        }

        private void UpdateBonusPresentation()
        {
            _spriteRenderer.color = Time.time < _bonusEndsAt
                ? new Color(1f, 0.72f, 0.35f, 1f)
                : Color.white;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.5f);
            Vector3 center = transform.position + Vector3.right * (_facing * saldırıUzaklığı);
            Gizmos.DrawWireCube(center, saldırıAlanı);
        }
    }
}
