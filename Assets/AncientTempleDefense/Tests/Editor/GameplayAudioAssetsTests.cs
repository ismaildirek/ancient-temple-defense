using System.Linq;
using AncientTempleDefense.Audio;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Tests
{
    public sealed class GameplayAudioAssetsTests
    {
        private const string GeneratedPrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";

        [Test]
        public void PlayerPrefabUsesOnlySwordSoundPackClips()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GeneratedPrefabRoot + "/BlackKnightPlayer.prefab");
            BlackKnightSwordAudio audio = prefab.GetComponent<BlackKnightSwordAudio>();

            Assert.That(audio, Is.Not.Null);
            string[] paths = audio.ConfiguredClips
                .Select(AssetDatabase.GetAssetPath)
                .Distinct()
                .ToArray();
            Assert.That(paths.Length, Is.GreaterThanOrEqualTo(12));
            Assert.That(paths.All(path => path.StartsWith("Assets/Musics/SwordSoundPack/")), Is.True);
            Assert.That(paths.Any(path => path.Contains("Leohpaz")), Is.False);
        }

        [TestCase("Skeleton")]
        [TestCase("Goblin")]
        [TestCase("Mushroom")]
        [TestCase("FlyingEye")]
        public void EnemyPrefabUsesOnlyLeohpazClips(string enemyName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{GeneratedPrefabRoot}/{enemyName}Enemy.prefab");
            EnemyAudioController audio = prefab.GetComponent<EnemyAudioController>();

            Assert.That(audio, Is.Not.Null);
            string[] paths = audio.ConfiguredClips
                .Select(AssetDatabase.GetAssetPath)
                .Distinct()
                .ToArray();
            Assert.That(paths.Length, Is.GreaterThanOrEqualTo(3));
            Assert.That(paths.All(path => path.StartsWith("Assets/Musics/Leohpaz/")), Is.True);
            Assert.That(paths.Any(path => path.Contains("SwordSoundPack")), Is.False);
        }

        [Test]
        public void MapSceneContainsLoopingBattleThemeOne()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(
                "Assets/Scenes/Map.unity",
                OpenSceneMode.Single);
            BattleMusicPlayer music = scene.GetRootGameObjects()
                .Select(root => root.GetComponent<BattleMusicPlayer>())
                .FirstOrDefault(component => component != null);

            Assert.That(music, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(music.MusicClip),
                Is.EqualTo("Assets/Musics/Battle Music Pack DEMO/Battle Theme 1_demo.wav"));
            Assert.That(music.GetComponent<AudioSource>().loop, Is.True);
        }

        [Test]
        public void PlayerSwordMixIsQuieterAndUsesSmoothReleaseWindows()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GeneratedPrefabRoot + "/BlackKnightPlayer.prefab");
            SerializedObject audio = new(prefab.GetComponent<BlackKnightSwordAudio>());

            SerializedProperty light = audio.FindProperty("hafifSaldırıSesleri");
            SerializedProperty heavy = audio.FindProperty("ağırSaldırıSesleri");
            SerializedProperty ultimate = audio.FindProperty("ultiSesleri");
            Assert.That(light.FindPropertyRelative("sesSeviyesi").floatValue, Is.EqualTo(0.50f).Within(0.001f));
            Assert.That(heavy.FindPropertyRelative("sesSeviyesi").floatValue, Is.EqualTo(0.60f).Within(0.001f));
            Assert.That(ultimate.FindPropertyRelative("sesSeviyesi").floatValue, Is.EqualTo(0.66f).Within(0.001f));
            Assert.That(light.FindPropertyRelative("yumuşakBitişSüresi").floatValue, Is.GreaterThanOrEqualTo(0.25f));
            Assert.That(heavy.FindPropertyRelative("yumuşakBitişSüresi").floatValue, Is.GreaterThanOrEqualTo(0.30f));
        }

        [Test]
public void AudioImportSettingsMatchRuntimeUse()
        {
            AudioImporter musicImporter = AssetImporter.GetAtPath(
                "Assets/Musics/Battle Music Pack DEMO/Battle Theme 1_demo.wav") as AudioImporter;
            AudioImporter swordImporter = AssetImporter.GetAtPath(
                "Assets/Musics/SwordSoundPack/SWORD_09.wav") as AudioImporter;

            Assert.That(musicImporter, Is.Not.Null);
            Assert.That(musicImporter.defaultSampleSettings.loadType, Is.EqualTo(AudioClipLoadType.Streaming));
            Assert.That(musicImporter.loadInBackground, Is.True);
            Assert.That(swordImporter, Is.Not.Null);
            Assert.That(swordImporter.defaultSampleSettings.loadType, Is.EqualTo(AudioClipLoadType.DecompressOnLoad));
            Assert.That(swordImporter.defaultSampleSettings.preloadAudioData, Is.True);
        }
    }
}
