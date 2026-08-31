using System;
using System.Collections.Generic;

using AncientTempleDefense.Audio;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class GameplayAudioSetup
    {
        private const string GeneratedPrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";
        private const string SwordRoot = "Assets/Musics/SwordSoundPack";
        private const string EnemyBattleRoot = "Assets/Musics/Leohpaz/RPG_Essentials_Free/10_Battle_SFX";
        private const string MusicPath = "Assets/Musics/Battle Music Pack DEMO/Battle Theme 1_demo.wav";
        private const string MapScenePath = "Assets/Scenes/Map.unity";

        private static readonly ClipDefinition[] LightSwordSounds =
        {
            Sword(9, 0.175f), Sword(24, 0.315f), Sword(25, 0.272f), Sword(27, 0.342f)
        };

        private static readonly ClipDefinition[] HeavySwordSounds =
        {
            Sword(3, 0.452f), Sword(7, 0.517f), Sword(8, 0.544f),
            Sword(19, 0.532f), Sword(20, 0.567f), Sword(21, 0.544f)
        };

        private static readonly ClipDefinition[] ParrySwordSounds =
        {
            Sword(1, 0.123f), Sword(2, 0.208f), Sword(5, 0.128f), Sword(11, 0.187f)
        };

        private static readonly ClipDefinition[] UltimateSwordSounds =
        {
            Sword(28, 0.289f), Sword(3, 0.452f), Sword(8, 0.544f)
        };

        private static readonly ClipDefinition[] DrawSwordSounds =
        {
            Sword(6, 0.363f), Sword(22, 0.282f), Sword(26, 0.472f)
        };

        private static readonly ClipDefinition[] SheatheSwordSounds =
        {
            Sword(10, 0.403f), Sword(23, 0.445f), Sword(27, 0.342f)
        };

        private static readonly ClipDefinition Slash = Battle("22_Slash_04.wav", 0.135f);
        private static readonly ClipDefinition Claw = Battle("03_Claw_03.wav", 0.127f);
        private static readonly ClipDefinition Bite = Battle("08_Bite_04.wav", 0.069f);
        private static readonly ClipDefinition FleshImpact = Battle("15_Impact_flesh_02.wav", 0.006f);
        private static readonly ClipDefinition FleshReaction = Battle("77_flesh_02.wav", 0.130f);
        private static readonly ClipDefinition Block = Battle("39_Block_03.wav", 0.013f);
        private static readonly ClipDefinition EnemyDeath = Battle("69_Enemy_death_01.wav", 0.035f);

        [MenuItem("Tools/Ancient Temple Defense/Configure Gameplay Audio")]
        public static void Configure()
        {
            ConfigureImportSettings();
            ConfigurePlayerPrefab();
            ConfigureEnemyPrefab("Skeleton");
            ConfigureEnemyPrefab("Goblin");
            ConfigureEnemyPrefab("Mushroom");
            ConfigureEnemyPrefab("FlyingEye");
            ConfigureSceneMusic();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Ancient Temple Defense ses kurulumu tamamlandi.");
        }

        private static void ConfigurePlayerPrefab()
        {
            string prefabPath = GeneratedPrefabRoot + "/BlackKnightPlayer.prefab";
            GameObject root = LoadPrefabContents(prefabPath);
            try
            {
                BlackKnightSwordAudio audio = root.GetComponent<BlackKnightSwordAudio>()
                    ?? root.AddComponent<BlackKnightSwordAudio>();
                SerializedObject serialized = new(audio);
                ConfigureClipSet(serialized, "hafifSaldırıSesleri", LightSwordSounds, 0.50f, 0.98f, 1.06f, 0.26f);
                ConfigureClipSet(serialized, "ağırSaldırıSesleri", HeavySwordSounds, 0.60f, 0.88f, 0.98f, 0.34f);
                ConfigureClipSet(serialized, "savunmaSesleri", ParrySwordSounds, 0.56f, 0.96f, 1.04f, 0.24f);
                ConfigureClipSet(serialized, "ultiSesleri", UltimateSwordSounds, 0.66f, 0.90f, 0.97f, 0.42f);
                ConfigureClipSet(serialized, "kılıçÇekmeSesleri", DrawSwordSounds, 0.46f, 0.96f, 1.04f, 0.26f);
                ConfigureClipSet(serialized, "kılıçKınlamaSesleri", SheatheSwordSounds, 0.43f, 0.96f, 1.04f, 0.22f);
                serialized.FindProperty("eşZamanlıSesSayısı").intValue = 4;
                serialized.FindProperty("üçBoyutluSesKarışımı").floatValue = 0.08f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureEnemyPrefab(string enemyName)
        {
            EnemyAudioProfile profile = EnemyAudioProfile.For(enemyName);
            string prefabPath = $"{GeneratedPrefabRoot}/{enemyName}Enemy.prefab";
            GameObject root = LoadPrefabContents(prefabPath);
            try
            {
                EnemyAudioController audio = root.GetComponent<EnemyAudioController>()
                    ?? root.AddComponent<EnemyAudioController>();
                SerializedObject serialized = new(audio);
                ConfigureClipSet(serialized, "birinciSaldırıSesleri", profile.AttackOne, profile.AttackVolume, 0.94f, 1.06f, 0.10f);
                ConfigureClipSet(serialized, "ikinciSaldırıSesleri", profile.AttackTwo, profile.AttackVolume, 0.92f, 1.08f, 0.10f);
                ConfigureClipSet(serialized, "hasarAlmaSesleri", profile.Hit, 0.72f, 0.94f, 1.08f, 0.12f);
                ConfigureClipSet(serialized, "ölümSesleri", profile.Death, 0.76f, 0.90f, 1.06f, 1.05f);
                ConfigureClipSet(serialized, "savunmaSesleri", profile.Defense, 0.66f, 0.96f, 1.04f, 0.16f);
                serialized.FindProperty("eşZamanlıSesSayısı").intValue = 3;
                serialized.FindProperty("üçBoyutluSesKarışımı").floatValue = 0.18f;
                serialized.FindProperty("saldırıTemasZamanı").floatValue = 0.42f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureSceneMusic()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MapScenePath, OpenSceneMode.Single);
            foreach (BattleMusicPlayer existing in UnityEngine.Object.FindObjectsByType<BattleMusicPlayer>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (existing.gameObject.scene == scene)
                {
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                }
            }

            GameObject gameAudio = new("GameAudio");
            SceneManager.MoveGameObjectToScene(gameAudio, scene);
            BattleMusicPlayer musicPlayer = gameAudio.AddComponent<BattleMusicPlayer>();
            AudioSource source = gameAudio.GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.volume = 0.24f;

            SerializedObject serialized = new(musicPlayer);
            serialized.FindProperty("müzikKlibi").objectReferenceValue = LoadAudioClip(MusicPath);
            serialized.FindProperty("sesSeviyesi").floatValue = 0.24f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
if (!EditorSceneManager.SaveScene(scene, MapScenePath))
            {
                throw new InvalidOperationException("Map sahnesine savas muzigi kaydedilemedi.");
            }
        }

        private static void ConfigureImportSettings()
        {
            HashSet<string> soundEffectPaths = new(StringComparer.Ordinal);
            AddPaths(soundEffectPaths, LightSwordSounds);
            AddPaths(soundEffectPaths, HeavySwordSounds);
            AddPaths(soundEffectPaths, ParrySwordSounds);
            AddPaths(soundEffectPaths, UltimateSwordSounds);
            AddPaths(soundEffectPaths, DrawSwordSounds);
            AddPaths(soundEffectPaths, SheatheSwordSounds);
            AddPaths(soundEffectPaths, new[] { Slash, Claw, Bite, FleshImpact, FleshReaction, Block, EnemyDeath });

            foreach (string path in soundEffectPaths)
            {
                ConfigureImporter(path, AudioClipLoadType.DecompressOnLoad, AudioCompressionFormat.PCM, true, false, 1f);
            }

            ConfigureImporter(MusicPath, AudioClipLoadType.Streaming, AudioCompressionFormat.Vorbis, false, true, 0.72f);
        }

        private static void ConfigureImporter(
            string path,
            AudioClipLoadType loadType,
            AudioCompressionFormat compressionFormat,
            bool preloadAudioData,
            bool loadInBackground,
            float quality)
        {
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter
                ?? throw new InvalidOperationException($"AudioImporter bulunamadi: {path}");
            AudioImporterSampleSettings current = importer.defaultSampleSettings;
            bool changed = current.loadType != loadType
                || current.compressionFormat != compressionFormat
                || !Mathf.Approximately(current.quality, quality)
                || current.preloadAudioData != preloadAudioData
                || importer.loadInBackground != loadInBackground;

            if (!changed)
            {
                return;
            }

            current.loadType = loadType;
            current.compressionFormat = compressionFormat;
            current.quality = quality;
            importer.defaultSampleSettings = current;
           importer.loadInBackground = loadInBackground;
            importer.SaveAndReimport();
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
                ?? throw new InvalidOperationException($"Ses seti alani bulunamadi: {propertyName}");
            SerializedProperty clips = set.FindPropertyRelative("sesKlipleri");
            clips.arraySize = definitions.Count;
            for (int index = 0; index < definitions.Count; index++)
            {
                SerializedProperty item = clips.GetArrayElementAtIndex(index);
                item.FindPropertyRelative("sesKlibi").objectReferenceValue = LoadAudioClip(definitions[index].Path);
                item.FindPropertyRelative("vuruşZirvesiSaniyesi").floatValue = definitions[index].PeakTimeSeconds;
            }

            set.FindPropertyRelative("sesSeviyesi").floatValue = volume;
            set.FindPropertyRelative("enDüşükPerde").floatValue = minimumPitch;
            set.FindPropertyRelative("enYüksekPerde").floatValue = maximumPitch;
            set.FindPropertyRelative("yumuşakBitişSüresi").floatValue = tailSeconds;
        }

        private static GameObject LoadPrefabContents(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                throw new InvalidOperationException($"Uretilmis prefab bulunamadi: {path}");
            }

            return PrefabUtility.LoadPrefabContents(path);
        }

        private static AudioClip LoadAudioClip(string path)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path)
                ?? throw new InvalidOperationException($"Ses klibi bulunamadi: {path}");
        }

        private static void AddPaths(ISet<string> destination, IEnumerable<ClipDefinition> definitions)
        {
            foreach (ClipDefinition definition in definitions)
            {
                destination.Add(definition.Path);
            }
        }

        private static ClipDefinition Sword(int number, float peakTimeSeconds)
        {
            return new ClipDefinition($"{SwordRoot}/SWORD_{number:00}.wav", peakTimeSeconds);
        }

        private static ClipDefinition Battle(string filename, float peakTimeSeconds)
        {
            return new ClipDefinition($"{EnemyBattleRoot}/{filename}", peakTimeSeconds);
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
                float attackVolume)
            {
                AttackOne = attackOne;
                AttackTwo = attackTwo;
                Hit = hit;
                Death = death;
                Defense = defense;
                AttackVolume = attackVolume;
            }

            public ClipDefinition[] AttackOne { get; }
            public ClipDefinition[] AttackTwo { get; }
            public ClipDefinition[] Hit { get; }
            public ClipDefinition[] Death { get; }
            public ClipDefinition[] Defense { get; }
            public float AttackVolume { get; }

            public static EnemyAudioProfile For(string enemyName)
            {
                return enemyName switch
                {
                    "Skeleton" => new EnemyAudioProfile(
                        new[] { Slash },
                        new[] { Slash },
                        new[] { Block },
                        new[] { EnemyDeath },
                        new[] { Block },
                        0.70f),
                    "Goblin" => new EnemyAudioProfile(
                        new[] { Slash },
                        new[] { Claw },
                        new[] { FleshImpact, FleshReaction },
                        new[] { EnemyDeath },
                        Array.Empty<ClipDefinition>(),
                        0.70f),
                    "Mushroom" => new EnemyAudioProfile(
                        new[] { Claw },
                        new[] { Bite },
                        new[] { FleshReaction, FleshImpact },
                        new[] { EnemyDeath },
                        Array.Empty<ClipDefinition>(),
                        0.66f),
                    "FlyingEye" => new EnemyAudioProfile(
                        new[] { Bite },
                        new[] { Claw },
                        new[] { FleshImpact, FleshReaction },
                        new[] { EnemyDeath },
                        Array.Empty<ClipDefinition>(),
                        0.68f),
                    _ => throw new ArgumentOutOfRangeException(nameof(enemyName), enemyName, "Bilinmeyen dusman ses profili.")
                };
            }
        }
    }
}
