using System.Collections;
using AncientTempleDefense.Progression;
using AncientTempleDefense.UI;
using AncientTempleDefense.Vfx;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientTempleDefense.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyWaveSpawner : MonoBehaviour
    {
        [Header("Düşmanlar ve Hedef")]
        [FormerlySerializedAs("enemyPrefabs")]
        [SerializeField, InspectorName("İlk Düşman Prefabları")] private EnemyCombatant[] düşmanPrefabları;
        [SerializeField, InspectorName("Geç Dönem Düşman Prefabları")] private EnemyCombatant[] geçDönemDüşmanPrefabları;
        [SerializeField, InspectorName("Geç Dönem Başlangıç Wave'i"), Min(1)] private int geçDönemBaşlangıçWave = 8;
        [SerializeField, InspectorName("Kurt Prefabı")] private EnemyCombatant kurtPrefabı;
        [SerializeField, InspectorName("Kurt Başlangıç Wave'i"), Min(1)] private int kurtBaşlangıçWave = 9;
        [FormerlySerializedAs("playerTarget")]
        [SerializeField, InspectorName("Oyuncu Hedefi")] private Transform oyuncuHedefi;

        [Header("Boss Wave'leri")]
        [SerializeField, InspectorName("Birinci Boss Prefabı")] private EnemyCombatant birinciBossPrefabı;
        [SerializeField, InspectorName("Birinci Boss Wave'i"), Min(1)] private int birinciBossWave = 7;
        [SerializeField, InspectorName("Birinci Boss Can Çarpanı"), Min(1f)] private float birinciBossCanÇarpanı = 4f;
        [SerializeField, InspectorName("Birinci Boss Hasar Çarpanı"), Min(1f)] private float birinciBossHasarÇarpanı = 1.4f;
        [SerializeField, InspectorName("Birinci Boss Hız Çarpanı"), Min(0.1f)] private float birinciBossHızÇarpanı = 1.05f;
        [SerializeField, InspectorName("İkinci Boss Prefabı")] private EnemyCombatant ikinciBossPrefabı;
        [SerializeField, InspectorName("İkinci Boss Wave'i"), Min(1)] private int ikinciBossWave = 12;
        [SerializeField, InspectorName("İkinci Boss Can Çarpanı"), Min(1f)] private float ikinciBossCanÇarpanı = 6f;
        [SerializeField, InspectorName("İkinci Boss Hasar Çarpanı"), Min(1f)] private float ikinciBossHasarÇarpanı = 1.8f;
        [SerializeField, InspectorName("İkinci Boss Hız Çarpanı"), Min(0.1f)] private float ikinciBossHızÇarpanı = 1.2f;
        [SerializeField, InspectorName("Üçüncü Boss Prefabı")] private EnemyCombatant üçüncüBossPrefabı;
        [SerializeField, InspectorName("Üçüncü Boss Wave'i"), Min(1)] private int üçüncüBossWave = 17;
        [SerializeField, InspectorName("Üçüncü Boss Can Çarpanı"), Min(1f)] private float üçüncüBossCanÇarpanı = 12f;
        [SerializeField, InspectorName("Üçüncü Boss Hasar Çarpanı"), Min(1f)] private float üçüncüBossHasarÇarpanı = 2.7f;
        [SerializeField, InspectorName("Üçüncü Boss Hız Çarpanı"), Min(0.1f)] private float üçüncüBossHızÇarpanı = 1.28f;
        [SerializeField, InspectorName("Dördüncü Boss Prefabı")] private EnemyCombatant dördüncüBossPrefabı;
        [SerializeField, InspectorName("Dördüncü Boss Wave'i"), Min(1)] private int dördüncüBossWave = 22;
        [SerializeField, InspectorName("Dördüncü Boss Can Çarpanı"), Min(1f)] private float dördüncüBossCanÇarpanı = 16f;
        [SerializeField, InspectorName("Dördüncü Boss Hasar Çarpanı"), Min(1f)] private float dördüncüBossHasarÇarpanı = 3.2f;
        [SerializeField, InspectorName("Dördüncü Boss Hız Çarpanı"), Min(0.1f)] private float dördüncüBossHızÇarpanı = 1.35f;

        [Header("Doğma Portalları")]
        [FormerlySerializedAs("leftSpawnPortal")]
        [SerializeField, InspectorName("Sol Portal")] private AmbientSpritePulse solDoğmaPortalı;
        [FormerlySerializedAs("rightSpawnPortal")]
        [SerializeField, InspectorName("Sağ Portal")] private AmbientSpritePulse sağDoğmaPortalı;
        [FormerlySerializedAs("leftSpawnX")]
        [SerializeField, InspectorName("Sol Doğma X Konumu")] private float solDoğmaXKonumu = -17f;
        [FormerlySerializedAs("rightSpawnX")]
        [SerializeField, InspectorName("Sağ Doğma X Konumu")] private float sağDoğmaXKonumu = 18f;
        [FormerlySerializedAs("groundSpawnY")]
        [SerializeField, InspectorName("Kara Düşmanı Y Konumu")] private float karaDüşmanıYKonumu = -3.25f;
        [FormerlySerializedAs("flyingSpawnY")]
        [SerializeField, InspectorName("Uçan Düşman Y Konumu")] private float uçanDüşmanYKonumu = -2.2f;

        [Header("Wave Ayarları")]
        [FormerlySerializedAs("startingWave")]
        [SerializeField, InspectorName("Başlangıç Wave'i"), Min(1)] private int başlangıçWave = 1;
        [FormerlySerializedAs("baseEnemiesPerWave")]
        [SerializeField, InspectorName("İlk Wave Düşman Sayısı"), Min(1)] private int ilkWaveDüşmanSayısı = 4;
        [FormerlySerializedAs("additionalEnemiesPerWave")]
        [SerializeField, InspectorName("Wave Başına Ek Düşman"), Min(0)] private int waveBaşınaEkDüşman = 2;
        [FormerlySerializedAs("spawnInterval")]
        [SerializeField, InspectorName("Düşman Doğma Aralığı"), Min(0.05f)] private float düşmanDoğmaAralığı = 0.45f;
        [FormerlySerializedAs("waveIntermission")]
        [SerializeField, InspectorName("Wave Arası Bekleme"), Min(0f)] private float waveArasıBekleme = 2f;
        [FormerlySerializedAs("maximumAlive")]
        [SerializeField, InspectorName("Aynı Anda En Fazla Düşman"), Min(1)] private int aynıAndaEnFazlaDüşman = 8;
        [FormerlySerializedAs("upgradeEveryWaves")]
        [SerializeField, InspectorName("Kaç Wave'de Bir Kart"), Min(1)] private int kaçWavedeBirKart = 5;
        [SerializeField, InspectorName("Kaç Wave'de Bir Mağaza"), Min(1)] private int kaçWavedeBirMağaza = 4;

        [Header("Düşman Güçlenmesi")]
        [FormerlySerializedAs("baseEnemyHealth")]
        [SerializeField, InspectorName("İlk Düşman Canı"), Min(1)] private int ilkDüşmanCanı = 3;
        [FormerlySerializedAs("healthGrowthPerWave")]
        [SerializeField, InspectorName("Wave Başına Can Artışı"), Range(0f, 1f)] private float waveBaşınaCanArtışı = 0.12f;
        [FormerlySerializedAs("baseEnemyDamage")]
        [SerializeField, InspectorName("İlk Düşman Hasarı"), Min(1)] private int ilkDüşmanHasarı = 8;
        [FormerlySerializedAs("damageGrowthPerWave")]
        [SerializeField, InspectorName("Wave Başına Hasar Artışı"), Range(0f, 1f)] private float waveBaşınaHasarArtışı = 0.10f;
        [FormerlySerializedAs("attackSpeedGrowthPerWave")]
        [SerializeField, InspectorName("Wave Başına Vuruş Hızı Artışı"), Range(0f, 1f)] private float waveBaşınaVuruşHızıArtışı = 0.04f;
        [FormerlySerializedAs("upgradePanel")]
        [SerializeField, InspectorName("Yükseltme Kartı Paneli")] private WaveUpgradePanel yükseltmeKartıPaneli;
        [SerializeField, InspectorName("Savunma Magazasi Paneli")] private DefenseShopPanel savunmaMagazasiPaneli;

        private int _nextEnemyIndex;
        private bool _spawnFromLeft = true;
        private int _currentEnemyHealth;
        private int _currentEnemyDamage;
        private float _currentAttackSpeed;
        private Collider2D _playerGroundCollider;
        private float _groundContactY;
        private bool _groundContactCaptured;

        public int CurrentWave { get; private set; }
        public int AliveEnemies { get; private set; }
        public int CurrentWaveTotal { get; private set; }
        public int CurrentEnemyHealth => _currentEnemyHealth;
        public int CurrentEnemyDamage => _currentEnemyDamage;
        public float CurrentAttackSpeedMultiplier => _currentAttackSpeed;
        public EnemyCombatant WolfPrefab => kurtPrefabı;
        public System.Collections.Generic.IReadOnlyList<EnemyCombatant> LateEnemyPrefabs =>
            geçDönemDüşmanPrefabları ?? System.Array.Empty<EnemyCombatant>();
        public bool IsWaitingForUpgrade => yükseltmeKartıPaneli != null && yükseltmeKartıPaneli.IsChoosing;

        public bool LateEnemiesUnlocked(int wave)
        {
            return WaveEnemyRosterRules.LateEnemiesUnlocked(wave, geçDönemBaşlangıçWave);
        }
        public bool WolfUnlocked(int wave)
        {
            return wave >= Mathf.Max(1, kurtBaşlangıçWave);
        }


        public EnemyCombatant BossPrefabForWave(int wave)
        {
            if (wave == birinciBossWave) return birinciBossPrefabı;
            if (wave == ikinciBossWave) return ikinciBossPrefabı;
            if (wave == üçüncüBossWave) return üçüncüBossPrefabı;
            return wave == dördüncüBossWave ? dördüncüBossPrefabı : null;
        }

        private IEnumerator Start()
        {
            if ((düşmanPrefabları == null || düşmanPrefabları.Length == 0)
                && birinciBossPrefabı == null
                && ikinciBossPrefabı == null
                && üçüncüBossPrefabı == null
                && dördüncüBossPrefabı == null)
            {
                yield break;
            }

            CaptureGroundContact();
            StartCoroutine(RefreshGroundContactAfterFirstPhysicsStep());

            CurrentWave = Mathf.Max(1, başlangıçWave);
            while (enabled)
            {
                PrepareWave(CurrentWave);
                yield return SpawnWave();
                yield return new WaitUntil(() => AliveEnemies <= 0);

                bool araEkranAçıldı = false;
                if (yükseltmeKartıPaneli != null && CurrentWave % Mathf.Max(1, kaçWavedeBirKart) == 0)
                {
                    yükseltmeKartıPaneli.ShowChoices(CurrentWave);
                    yield return new WaitUntil(() => !yükseltmeKartıPaneli.IsChoosing);
                    araEkranAçıldı = true;
                }

                if (savunmaMagazasiPaneli != null && CurrentWave % Mathf.Max(1, kaçWavedeBirMağaza) == 0)
                {
                    savunmaMagazasiPaneli.ShowShop(CurrentWave);
                    yield return new WaitUntil(() => !savunmaMagazasiPaneli.IsOpen);
                    araEkranAçıldı = true;
                }

                if (!araEkranAçıldı && waveArasıBekleme > 0f)
                {
                    yield return new WaitForSeconds(waveArasıBekleme);
                }

                CurrentWave++;
            }
        }

        private void PrepareWave(int wave)
        {
            CurrentWaveTotal = BossPrefabForWave(wave) != null
                ? 1
                : WaveScaling.EnemyCount(wave, ilkWaveDüşmanSayısı, waveBaşınaEkDüşman);
            _currentEnemyHealth = WaveScaling.EnemyHealth(wave, ilkDüşmanCanı, waveBaşınaCanArtışı);
            _currentEnemyDamage = WaveScaling.EnemyDamage(wave, ilkDüşmanHasarı, waveBaşınaHasarArtışı);
            _currentAttackSpeed = WaveScaling.AttackSpeedMultiplier(wave, waveBaşınaVuruşHızıArtışı);
            AliveEnemies = 0;
            RefreshStatus();
        }

        private IEnumerator SpawnWave()
        {
            if (TryGetBossConfiguration(CurrentWave, out BossSpawnConfiguration boss))
            {
                yield return new WaitUntil(() => AliveEnemies < Mathf.Max(1, aynıAndaEnFazlaDüşman));
                SpawnPrefab(boss.Prefab, boss.Health, boss.Damage, boss.AttackSpeed);
                RefreshStatus();
                yield break;
            }

            for (int index = 0; index < CurrentWaveTotal; index++)
            {
                yield return new WaitUntil(() => AliveEnemies < Mathf.Max(1, aynıAndaEnFazlaDüşman));
                SpawnNext();
                RefreshStatus();

                if (index < CurrentWaveTotal - 1)
                {
                    yield return new WaitForSeconds(düşmanDoğmaAralığı);
                }
            }
        }

        private void SpawnNext()
        {
            EnemyCombatant prefab = FindNextPrefab(CurrentWave);
            if (prefab != null)
            {
                SpawnPrefab(prefab, _currentEnemyHealth, _currentEnemyDamage, _currentAttackSpeed);
            }
        }

        private void SpawnPrefab(EnemyCombatant prefab, int health, int damage, float attackSpeed)
        {
            bool flying = prefab.name.Contains("Flying", System.StringComparison.OrdinalIgnoreCase);
            EnemyRoleProfile role = prefab.GetComponent<EnemyRoleProfile>();
            if (role != null)
            {
                health = Mathf.Max(1, Mathf.RoundToInt(health * role.HealthMultiplier));
                damage = Mathf.Max(1, Mathf.RoundToInt(damage * role.DamageMultiplier));
                attackSpeed = Mathf.Max(0.1f, attackSpeed * role.AttackSpeedMultiplier);
            }

            bool spawnFromLeft = _spawnFromLeft;
            float x = spawnFromLeft ? solDoğmaXKonumu : sağDoğmaXKonumu;
            flying = role != null ? role.IsFlying : flying;
            _spawnFromLeft = !_spawnFromLeft;
            (spawnFromLeft ? solDoğmaPortalı : sağDoğmaPortalı)?.Burst();

            EnemyCombatant enemy = Instantiate(
                prefab,
                new Vector3(
                    x,
                    flying ? uçanDüşmanYKonumu + prefab.SpawnYOffset : ResolveGroundContactY(),
                    0f),
                Quaternion.identity,
                transform);
            if (!flying)
            {
                AlignEnemyGroundContact(enemy, prefab.SpawnYOffset);
            }

            enemy.ConfigureForWave(health);
            enemy.Died += OnEnemyDied;
            AliveEnemies++;

            EnemyBrain brain = enemy.GetComponent<EnemyBrain>();
            if (brain != null)
            {
                brain.Initialize(oyuncuHedefi, damage, attackSpeed);
            }

            BossEnemyBrain bossBrain = enemy.GetComponent<BossEnemyBrain>();
            if (bossBrain != null)
            {
                bossBrain.Initialize(oyuncuHedefi, damage, attackSpeed);
            }
        }

        private float ResolveGroundContactY()
        {
            if (!_groundContactCaptured)
            {
                CaptureGroundContact();
            }

            return _groundContactY;
        }

        private void CaptureGroundContact()
        {
            if (_playerGroundCollider == null && oyuncuHedefi != null)
            {
                _playerGroundCollider = oyuncuHedefi.GetComponent<Collider2D>();
            }

            _groundContactY = _playerGroundCollider != null
                ? _playerGroundCollider.bounds.min.y
                : karaDüşmanıYKonumu;
            _groundContactCaptured = true;
        }

        private IEnumerator RefreshGroundContactAfterFirstPhysicsStep()
        {
            yield return new WaitForFixedUpdate();
            CaptureGroundContact();

            for (int index = 0; index < EnemyCombatant.ActiveEnemies.Count; index++)
            {
                EnemyCombatant enemy = EnemyCombatant.ActiveEnemies[index];
                if (enemy == null
                    || !enemy.transform.IsChildOf(transform)
                    || enemy.name.Contains("Flying", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AlignEnemyGroundContact(enemy, enemy.SpawnYOffset);
            }
        }

        private void AlignEnemyGroundContact(EnemyCombatant enemy, float manualOffset)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider == null)
            {
                return;
            }

            float correction = ResolveGroundContactY() - enemyCollider.bounds.min.y + manualOffset;
            enemy.transform.position += Vector3.up * correction;
        }

        private EnemyCombatant FindNextPrefab(int wave)
        {
            int earlyCount = düşmanPrefabları?.Length ?? 0;
            int lateCount = LateEnemiesUnlocked(wave) ? geçDönemDüşmanPrefabları?.Length ?? 0 : 0;
            if (WolfUnlocked(wave) && kurtPrefabı != null && _nextEnemyIndex % 5 == 0)
            {
                _nextEnemyIndex++;
                return kurtPrefabı;
            }

            int totalCount = earlyCount + lateCount;
            for (int offset = 0; offset < totalCount; offset++)
            {
                int index = _nextEnemyIndex++ % totalCount;
                EnemyCombatant candidate = index < earlyCount
                    ? düşmanPrefabları[index]
                    : geçDönemDüşmanPrefabları[index - earlyCount];
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        private bool TryGetBossConfiguration(int wave, out BossSpawnConfiguration configuration)
        {
            if (wave == birinciBossWave && birinciBossPrefabı != null)
            {
                configuration = CreateBossConfiguration(
                    birinciBossPrefabı,
                    birinciBossCanÇarpanı,
                    birinciBossHasarÇarpanı,
                    birinciBossHızÇarpanı);
                return true;
            }

            if (wave == ikinciBossWave && ikinciBossPrefabı != null)
            {
                configuration = CreateBossConfiguration(
                    ikinciBossPrefabı,
                    ikinciBossCanÇarpanı,
                    ikinciBossHasarÇarpanı,
                    ikinciBossHızÇarpanı);
                return true;
            }

            if (wave == üçüncüBossWave && üçüncüBossPrefabı != null)
            {
                configuration = CreateBossConfiguration(
                    üçüncüBossPrefabı,
                    üçüncüBossCanÇarpanı,
                    üçüncüBossHasarÇarpanı,
                    üçüncüBossHızÇarpanı);
                return true;
            }

            if (wave == dördüncüBossWave && dördüncüBossPrefabı != null)
            {
                configuration = CreateBossConfiguration(
                    dördüncüBossPrefabı,
                    dördüncüBossCanÇarpanı,
                    dördüncüBossHasarÇarpanı,
                    dördüncüBossHızÇarpanı);
                return true;
            }

            configuration = default;
            return false;
        }

        private BossSpawnConfiguration CreateBossConfiguration(
            EnemyCombatant prefab,
            float healthMultiplier,
            float damageMultiplier,
            float speedMultiplier)
        {
            return new BossSpawnConfiguration(
                prefab,
                Mathf.Max(1, Mathf.RoundToInt(_currentEnemyHealth * healthMultiplier)),
                Mathf.Max(1, Mathf.RoundToInt(_currentEnemyDamage * damageMultiplier)),
                Mathf.Max(0.1f, _currentAttackSpeed * speedMultiplier));
        }

        private void OnEnemyDied(EnemyCombatant enemy)
        {
            enemy.Died -= OnEnemyDied;
            AliveEnemies = Mathf.Max(0, AliveEnemies - 1);
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            yükseltmeKartıPaneli?.UpdateWaveStatus(CurrentWave);
        }

        private readonly struct BossSpawnConfiguration
        {
            public BossSpawnConfiguration(EnemyCombatant prefab, int health, int damage, float attackSpeed)
            {
                Prefab = prefab;
                Health = health;
                Damage = damage;
                AttackSpeed = attackSpeed;
            }

            public EnemyCombatant Prefab { get; }
            public int Health { get; }
            public int Damage { get; }
            public float AttackSpeed { get; }
        }
    }
}
