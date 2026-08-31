using System;
using System.Collections.Generic;
using AncientTempleDefense.Audio;
using UnityEditor;
using UnityEngine;

namespace AncientTempleDefense.Editor
{
    internal static class LateGameEnemyAudioSetup
    {
        private const string GeneratedPrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";
        private const string BattleRoot = "Assets/Musics/Leohpaz/RPG_Essentials_Free/10_Battle_SFX";
        private const string MagicRoot = "Assets/Musics/Leohpaz/RPG_Essentials_Free/8_Atk_Magic_SFX";

        private static readonly ClipDefinition Slash = Battle("22_Slash_04.wav", 0.135f);
        private static readonly ClipDefinition Claw = Battle("03_Claw_03.wav", 0.127f);
        private static readonly ClipDefinition Bite = Battle("08_Bite_04.wav", 0.069f);
        private static readonly ClipDefinition FleshImpact = Battle("15_Impact_flesh_02.wav", 0.006f);
        private static readonly ClipDefinition FleshReaction = Battle("77_flesh_02.wav", 0.130f);
        private static readonly ClipDefinition Block = Battle("39_Block_03.wav", 0.013f);
        private static readonly ClipDefinition EnemyDeath = Battle("69_Enemy_death_01.wav", 0.035f);
        private static readonly ClipDefinition FireExplosion = Magic("04_Fire_explosion_04_medium.wav", 0.120f);
        private static readonly ClipDefinition Thunder = Magic("18_Thunder_02.wav", 0.090f);
        private static readonly ClipDefinition Wind = Magic("25_Wind_01.wav", 0.100f);
        private static readonly ClipDefinition Earth = Magic("30_Earth_02.wav", 0.125f);
        private static readonly ClipDefinition Charge = Magic("45_Charge_05.wav", 0.110f);
        private static readonly ClipDefinition Poison = Magic("46_Poison_01.wav", 0.105f);

        public static void Configure()
        {
            ConfigureImportSettings();
            foreach (string enemyName in new[]
                     {
                         "NewEnemy1", "NewEnemy2", "NewEnemy3", "NewEnemy4", "NewEnemy5", "Boss1", "Boss2"
                     })
            {
                ConfigurePrefab(enemyName);
            }
        }

        private static void ConfigurePrefab(string enemyName)
        {
            string prefabPath = $"{GeneratedPrefabRoot}/{enemyName}.prefab";
            if (enemyName.StartsWith("Boss", StringComparison.Ordinal))
            {
                prefabPath = $"{GeneratedPrefabRoot}/{enemyName}Enemy.prefab";
            }

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                EnemyAudioProfile profile = EnemyAudioProfile.For(enemyName);
                EnemyAudioController audio = root.GetComponent<EnemyAudioController>()
                    ?? root.AddComponent<EnemyAudioController>();
                SerializedObject serialized = new(audio);
                ConfigureClipSet(serialized, "birinciSaldırıSesleri", profile.AttackOne, profile.AttackVolume, 0.92f, 1.08f, 0.12f);
                ConfigureClipSet(serialized, "ikinciSaldırıSesleri", profile.AttackTwo, profile.AttackVolume, 0.90f, 1.08f, 0.14f);
                ConfigureClipSet(serialized, "hasarAlmaSesleri", profile.Hit, 0.72f, 0.92f, 1.08f, 0.14f);
                ConfigureClipSet(serialized, "ölümSesleri", profile.Death, 0.78f, 0.88f, 1.04f, 1.05f);
                ConfigureClipSet(serialized, "savunmaSesleri", profile.Defense, 0.68f, 0.94f, 1.04f, 0.18f);
                ConfigureClipSet(serialized, "özelSaldırıSesleri", profile.Special, profile.SpecialVolume, 0.90f, 1.04f, 0.26f);
                ConfigureClipSet(serialized, "güçlüÖzelSaldırıSesleri", profile.PowerfulSpecial, profile.SpecialVolume, 0.86f, 1.00f, 0.36f);
                serialized.FindProperty("eşZamanlıSesSayısı").intValue = enemyName.StartsWith("Boss", StringComparison.Ordinal) ? 5 : 3;
                serialized.FindProperty("üçBoyutluSesKarışımı").floatValue = enemyName.StartsWith("Boss", StringComparison.Ordinal) ? 0.22f : 0.18f;
                serialized.FindProperty("saldırıTemasZamanı").floatValue = enemyName == "Boss2" ? 0.64f : 0.48f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureImportSettings()
        {
            HashSet<string> paths = new(StringComparer.Ordinal);
            foreach (string enemyName in new[]
                     {
                         "NewEnemy1", "NewEnemy2", "NewEnemy3", "NewEnemy4", "NewEnemy5", "Boss1", "Boss2"
                     })
            {
                EnemyAudioProfile profile = EnemyAudioProfile.For(enemyName);
                AddPaths(paths, profile.AttackOne);
                AddPaths(paths, profile.AttackTwo);
                AddPaths(paths, profile.Hit);
                AddPaths(paths, profile.Death);
                AddPaths(paths, profile.Defense);
                AddPaths(paths, profile.Special);
                AddPaths(paths, profile.PowerfulSpecial);
            }

            foreach (string path in paths)
            {
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter
                    ?? throw new InvalidOperationException($"AudioImporter bulunamadı: {path}");
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                bool changed = settings.loadType != AudioClipLoadType.DecompressOnLoad
                    || settings.compressionFormat != AudioCompressionFormat.PCM
                    || !settings.preloadAudioData
                    || importer.loadInBackground;
                if (!changed)
                {
                    continue;
                }

                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                settings.preloadAudioData = true;
                settings.quality = 1f;
                importer.defaultSampleSettings = settings;
                importer.loadInBackground = false;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureClipSet(
            SerializedObject owner,
            string propertyName,
            IReadOnlyList<ClipDefinition> definitions,
            float volume,
            float minimumPitch,
            float maximumPitch,
            float tailSeconds)
        {
            SerializedProperty set = owner.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"Ses seti alanı bulunamadı: {propertyName}");
            SerializedProperty clips = set.FindPropertyRelative("sesKlipleri");
            clips.arraySize = definitions.Count;
            for (int index = 0; index < definitions.Count; index++)
            {
                SerializedProperty item = clips.GetArrayElementAtIndex(index);
                item.FindPropertyRelative("sesKlibi").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<AudioClip>(definitions[index].Path)
                    ?? throw new InvalidOperationException($"Ses klibi bulunamadı: {definitions[index].Path}");
                item.FindPropertyRelative("vuruşZirvesiSaniyesi").floatValue = definitions[index].PeakTimeSeconds;
            }

            set.FindPropertyRelative("sesSeviyesi").floatValue = volume;
            set.FindPropertyRelative("enDüşükPerde").floatValue = minimumPitch;
            set.FindPropertyRelative("enYüksekPerde").floatValue = maximumPitch;
            set.FindPropertyRelative("yumuşakBitişSüresi").floatValue = tailSeconds;
        }

        private static void AddPaths(ISet<string> destination, IEnumerable<ClipDefinition> definitions)
        {
            foreach (ClipDefinition definition in definitions)
            {
                destination.Add(definition.Path);
            }
        }

        private static ClipDefinition Battle(string filename, float peakTimeSeconds)
        {
            return new ClipDefinition($"{BattleRoot}/{filename}", peakTimeSeconds);
        }

        private static ClipDefinition Magic(string filename, float peakTimeSeconds)
        {
            return new ClipDefinition($"{MagicRoot}/{filename}", peakTimeSeconds);
        }

        private readonly struct ClipDefinition
        {
            public ClipDefinition(string path, float peakTimeSeconds)
            {
                Path = path;
                PeakTimeSeconds = peakTimeSeconds;
            }

            public string Path { get; }
            public float PeakTimeSeconds { get; }
        }

        private readonly struct EnemyAudioProfile
        {
            private EnemyAudioProfile(
                ClipDefinition[] attackOne,
                ClipDefinition[] attackTwo,
                ClipDefinition[] hit,
                ClipDefinition[] death,
                ClipDefinition[] defense,
                ClipDefinition[] special,
                ClipDefinition[] powerfulSpecial,
                float attackVolume,
                float specialVolume)
            {
                AttackOne = attackOne;
                AttackTwo = attackTwo;
                Hit = hit;
                Death = death;
                Defense = defense;
                Special = special;
                PowerfulSpecial = powerfulSpecial;
                AttackVolume = attackVolume;
                SpecialVolume = specialVolume;
            }

            public ClipDefinition[] AttackOne { get; }
            public ClipDefinition[] AttackTwo { get; }
            public ClipDefinition[] Hit { get; }
            public ClipDefinition[] Death { get; }
            public ClipDefinition[] Defense { get; }
            public ClipDefinition[] Special { get; }
            public ClipDefinition[] PowerfulSpecial { get; }
            public float AttackVolume { get; }
            public float SpecialVolume { get; }

            public static EnemyAudioProfile For(string enemyName)
            {
                ClipDefinition[] none = Array.Empty<ClipDefinition>();
                return enemyName switch
                {
                    "NewEnemy1" => new EnemyAudioProfile(new[] { Slash }, new[] { Claw }, new[] { FleshImpact }, new[] { EnemyDeath }, none, none, none, 0.68f, 0.72f),
                    "NewEnemy2" => new EnemyAudioProfile(new[] { Slash }, new[] { Slash }, new[] { Block, FleshImpact }, new[] { EnemyDeath }, new[] { Block }, none, none, 0.70f, 0.72f),
                    "NewEnemy3" => new EnemyAudioProfile(new[] { Claw }, new[] { FireExplosion, Poison }, new[] { FleshReaction }, new[] { EnemyDeath }, none, new[] { Wind }, none, 0.68f, 0.76f),
                    "NewEnemy4" => new EnemyAudioProfile(new[] { Bite }, new[] { Claw }, new[] { FleshImpact }, new[] { EnemyDeath }, none, none, none, 0.68f, 0.72f),
                    "NewEnemy5" => new EnemyAudioProfile(new[] { Slash }, new[] { Claw }, new[] { FleshReaction }, new[] { EnemyDeath }, none, none, none, 0.72f, 0.74f),
                    "Boss1" => new EnemyAudioProfile(new[] { Slash, Claw }, new[] { Charge }, new[] { FleshImpact, FleshReaction }, new[] { EnemyDeath }, none, new[] { FireExplosion, Thunder }, new[] { Earth }, 0.78f, 0.86f),
                    "Boss2" => new EnemyAudioProfile(new[] { Slash, Claw }, new[] { FireExplosion, Thunder }, new[] { FleshImpact, FleshReaction }, new[] { EnemyDeath }, none, new[] { Charge, Wind }, new[] { Earth, Thunder }, 0.82f, 0.92f),
                    _ => throw new ArgumentOutOfRangeException(nameof(enemyName), enemyName, "Bilinmeyen geç dönem düşman ses profili.")
                };
            }
        }
    }
}
