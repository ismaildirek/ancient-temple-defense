using System.Collections.Generic;
using AncientTempleDefense.Allies;
using AncientTempleDefense.Player;
using AncientTempleDefense.Temple;
using AncientTempleDefense.Vfx;
using UnityEngine;

namespace AncientTempleDefense.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyCombatant), typeof(EnemyBrain))]
    public sealed class ExplodingEnemy : MonoBehaviour
    {
        [Header("Ölüm Patlaması")]
        [SerializeField, InspectorName("Patlama Yarıçapı"), Min(0.1f)] private float patlamaYarıçapı = 2.2f;
        [SerializeField, InspectorName("Patlama Hasar Çarpanı"), Min(0.1f)] private float patlamaHasarÇarpanı = 1.6f;
        [SerializeField, InspectorName("Patlama Efekt Çarpanı"), Min(0.1f)] private float patlamaEfektÇarpanı = 0.70f;

        private readonly Collider2D[] _overlaps = new Collider2D[32];
        private readonly HashSet<int> _damagedTargets = new();
        private EnemyCombatant _combatant;
        private EnemyBrain _brain;
        private EnemyVfxController _effects;
        private ContactFilter2D _filter;
        private bool _exploded;

        public float ExplosionRadius => patlamaYarıçapı;
        public float ExplosionEffectMultiplier => patlamaEfektÇarpanı;
        public int ExplosionCount { get; private set; }
        public int LastExplosionDamage { get; private set; }

        private void Awake()
        {
            _combatant = GetComponent<EnemyCombatant>();
            _brain = GetComponent<EnemyBrain>();
            _effects = GetComponent<EnemyVfxController>();
            _filter = ContactFilter2D.noFilter;
            _filter.useTriggers = true;
        }

        private void OnEnable()
        {
            _exploded = false;
            if (_combatant != null)
            {
                _combatant.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (_combatant != null)
            {
                _combatant.Died -= OnDied;
            }
        }

        private void OnDied(EnemyCombatant combatant)
        {
            if (_exploded)
            {
                return;
            }

            _exploded = true;
            ExplosionCount++;
            LastExplosionDamage = Mathf.Max(
                1,
                Mathf.RoundToInt((_brain != null ? _brain.AttackDamage : 1) * patlamaHasarÇarpanı));
            _effects?.PlaySpecial(transform.position, patlamaEfektÇarpanı);

            int count = Physics2D.OverlapCircle(transform.position, patlamaYarıçapı, _filter, _overlaps);
            _damagedTargets.Clear();
            for (int index = 0; index < count; index++)
            {
                Collider2D overlap = _overlaps[index];
                _overlaps[index] = null;
                if (overlap == null)
                {
                    continue;
                }

                PlayerHealth player = overlap.GetComponentInParent<PlayerHealth>();
                if (player != null && _damagedTargets.Add(player.GetInstanceID()))
                {
                    player.TakeDamage(LastExplosionDamage);
                    continue;
                }

                TempleHealth temple = overlap.GetComponentInParent<TempleHealth>();
                if (temple != null && _damagedTargets.Add(temple.GetInstanceID()))
                {
                    temple.TakeDamage(LastExplosionDamage);
                    continue;
                }

                FriendlyDefender defender = overlap.GetComponentInParent<FriendlyDefender>();
                if (defender != null && _damagedTargets.Add(defender.GetInstanceID()))
                {
                    defender.TakeDamage(LastExplosionDamage);
                }
            }
        }
    }
}
