using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientTempleDefense.Audio
{
[DisallowMultipleComponent]
    public sealed class BlackKnightSwordAudio : MonoBehaviour
    {
        [Header("K\u0131l\u0131\u00e7 Eylemleri")]
        [FormerlySerializedAs("lightAttackSounds")]
        [SerializeField, InspectorName("Hafif Sald\u0131r\u0131 Sesleri")] private TimedAudioClipSet hafifSaldırıSesleri = new();
        [FormerlySerializedAs("heavyAttackSounds")]
        [SerializeField, InspectorName("A\u011f\u0131r Sald\u0131r\u0131 Sesleri")] private TimedAudioClipSet ağırSaldırıSesleri = new();
        [FormerlySerializedAs("parrySounds")]
        [SerializeField, InspectorName("Savunma Sesleri")] private TimedAudioClipSet savunmaSesleri = new();
        [FormerlySerializedAs("ultimateSounds")]
        [SerializeField, InspectorName("Ulti Sesleri")] private TimedAudioClipSet ultiSesleri = new();
        [FormerlySerializedAs("drawSounds")]
        [SerializeField, InspectorName("K\u0131l\u0131\u00e7 \u00c7ekme Sesleri")] private TimedAudioClipSet kılıçÇekmeSesleri = new();
        [FormerlySerializedAs("sheatheSounds")]
        [SerializeField, InspectorName("K\u0131l\u0131\u00e7 K\u0131nlama Sesleri")] private TimedAudioClipSet kılıçKınlamaSesleri = new();

        [Header("Oynatma")]
        [FormerlySerializedAs("voiceCount")]
        [SerializeField, InspectorName("E\u015f Zamanl\u0131 Ses Say\u0131s\u0131"), Range(1, 8)] private int eşZamanlıSesSayısı = 4;
        [FormerlySerializedAs("spatialBlend")]
        [SerializeField, InspectorName("3B Ses Kar\u0131\u015f\u0131m\u0131"), Range(0f, 1f)] private float üçBoyutluSesKarışımı = 0.08f;

        private AudioVoicePool _voices;
        public AudioClip LastPlayedClip => _voices?.LastPlayedClip;
        public float LastScheduledDuration { get; private set; }

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

        public void StopPlayback()
        {
            _voices?.StopAll();
        }

        public void PlayAttack(bool heavy, float animationDuration, float contactNormalizedTime)
        {
            Play(heavy ? ağırSaldırıSesleri : hafifSaldırıSesleri, animationDuration, contactNormalizedTime);        }

        public void PlayParry(float animationDuration)
        {
            Play(savunmaSesleri, animationDuration, 0.22f);
        }

        public void PlayUltimate(float animationDuration, float contactNormalizedTime)
        {
            Play(ultiSesleri, animationDuration, contactNormalizedTime);
        }

        public void PlayWeaponToggle(bool drawing, float animationDuration)
        {
            Play(drawing ? kılıçÇekmeSesleri : kılıçKınlamaSesleri, animationDuration, 0.28f);
        }

        private void Play(TimedAudioClipSet set, float animationDuration, float contactNormalizedTime)
        {
            LastScheduledDuration = _voices?.PlayAligned(
                set,
                Mathf.Max(0f, animationDuration) * Mathf.Clamp01(contactNormalizedTime),
                Mathf.Max(0.05f, animationDuration),
                üçBoyutluSesKarışımı) ?? 0f;
        }

        private IEnumerable<TimedAudioClipSet> AllSets()
        {
            yield return hafifSaldırıSesleri;
            yield return ağırSaldırıSesleri;
            yield return savunmaSesleri;
            yield return ultiSesleri;
            yield return kılıçÇekmeSesleri;
            yield return kılıçKınlamaSesleri;
        }
    }
}