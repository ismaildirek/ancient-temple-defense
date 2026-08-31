using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientTempleDefense.Audio
{
[DisallowMultipleComponent]
    public sealed class EnemyAudioController : MonoBehaviour
    {
        [Header("Enemy Actions")]
        [FormerlySerializedAs("attackOneSounds")]
        [SerializeField] private TimedAudioClipSet birinciSaldırıSesleri = new();
        [FormerlySerializedAs("attackTwoSounds")]
        [SerializeField] private TimedAudioClipSet ikinciSaldırıSesleri = new();
        [FormerlySerializedAs("hitSounds")]
        [SerializeField] private TimedAudioClipSet hasarAlmaSesleri = new();
        [FormerlySerializedAs("deathSounds")]
        [SerializeField] private TimedAudioClipSet ölümSesleri = new();
        [FormerlySerializedAs("defenseSounds")]
        [SerializeField] private TimedAudioClipSet savunmaSesleri = new();
        [SerializeField] private TimedAudioClipSet özelSaldırıSesleri = new();
        [SerializeField] private TimedAudioClipSet güçlüÖzelSaldırıSesleri = new();

        [Header("Playback")]
        [FormerlySerializedAs("voiceCount")]
        [SerializeField, Range(1, 8)] private int eşZamanlıSesSayısı = 3;
        [FormerlySerializedAs("spatialBlend")]
        [SerializeField, Range(0f, 1f)] private float üçBoyutluSesKarışımı = 0.18f;
        [FormerlySerializedAs("attackContactNormalizedTime")]
        [SerializeField, Range(0f, 1f)] private float saldırıTemasZamanı = 0.42f;

        private AudioVoicePool _voices;
        public AudioClip LastPlayedClip => _voices?.LastPlayedClip;

        public IEnumerable<AudioClip> ConfiguredClips
        {
            get
            {
                foreach (TimedAudioClipSet set in AllSets())
                {
                    foreach (AudioClip clip in set.ConfiguredClips)
                    {
                        yield return clip;
                    }
                }
            }
        }

        private void Awake()
        {
            _voices = new AudioVoicePool(this, eşZamanlıSesSayısı);
        }

        private void OnDisable()
        {
            _voices?.StopAll();
        }

        public void PlayAction(string stateName, float animationDuration)
        {
            TimedAudioClipSet set = stateName switch
            {
                "Attack1" or "Attack" => birinciSaldırıSesleri,
                "Attack2" or "Cast" or "Attack3" => ikinciSaldırıSesleri,
                "Spell" or "Special1" or "Jump" => özelSaldırıSesleri,
                "Special2" => güçlüÖzelSaldırıSesleri,
                "Shield" => savunmaSesleri,
                _ => null
            };
            if (set == null)
            {
                return;
            }

            float contactTime = stateName == "Shield" ? 0.12f : saldırıTemasZamanı;
            _voices?.PlayAligned(
                set,
                Mathf.Max(0f, animationDuration) * contactTime,
                Mathf.Max(0.05f, animationDuration),
                üçBoyutluSesKarışımı);
        }

        public void PlayHit(float animationDuration)
        {
            _voices?.PlayAligned(hasarAlmaSesleri, 0.04f, Mathf.Max(0.12f, animationDuration), üçBoyutluSesKarışımı);
        }

        public float PlayDeath(float animationDuration)
        {
            return _voices?.PlayAligned(
                ölümSesleri,
                0.06f,
                Mathf.Max(0.25f, animationDuration),
                üçBoyutluSesKarışımı) ?? 0f;
        }

        private IEnumerable<TimedAudioClipSet> AllSets()
        {
            yield return birinciSaldırıSesleri;
            yield return ikinciSaldırıSesleri;
            yield return hasarAlmaSesleri;
            yield return ölümSesleri;
            yield return savunmaSesleri;
            yield return özelSaldırıSesleri;
            yield return güçlüÖzelSaldırıSesleri;
        }
    }
}