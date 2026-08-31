using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AncientTempleDefense.Audio
{
    [Serializable]
    public struct TimedAudioClip
    {
        [FormerlySerializedAs("clip")]
        [SerializeField, InspectorName("Ses Klibi")] private AudioClip sesKlibi;
        [FormerlySerializedAs("peakTimeSeconds")]
        [SerializeField, InspectorName("Vuru\u015f Zirvesi"), Min(0f)] private float vuruşZirvesiSaniyesi;

        public AudioClip Clip => sesKlibi;
        public float PeakTimeSeconds => vuruşZirvesiSaniyesi;
    }

    [Serializable]
    public sealed class TimedAudioClipSet
    {
        [FormerlySerializedAs("clips")]
        [SerializeField, InspectorName("Ses Klipleri")] private TimedAudioClip[] sesKlipleri = Array.Empty<TimedAudioClip>();
        [FormerlySerializedAs("volume")]
        [SerializeField, InspectorName("Ses Seviyesi"), Range(0f, 1f)] private float sesSeviyesi = 1f;
        [FormerlySerializedAs("minimumPitch")]
        [SerializeField, InspectorName("En D\u00fc\u015f\u00fck Perde"), Range(0.5f, 1.5f)] private float enDüşükPerde = 0.96f;
        [FormerlySerializedAs("maximumPitch")]
        [SerializeField, InspectorName("En Y\u00fcksek Perde"), Range(0.5f, 1.5f)] private float enYüksekPerde = 1.04f;
        [FormerlySerializedAs("tailSeconds")]
        [SerializeField, InspectorName("Yumu\u015fak Biti\u015f S\u00fcresi"), Min(0f)] private float yumuşakBitişSüresi = 0.15f;

        public int Count => sesKlipleri?.Length ?? 0;

        public IEnumerable<AudioClip> ConfiguredClips
        {
            get
            {
                if (sesKlipleri == null)
                {
                    yield break;
                }

                foreach (TimedAudioClip timedClip in sesKlipleri)
                {
                    if (timedClip.Clip != null)
                    {                        yield return timedClip.Clip;
                    }
                }
            }
        }

        internal bool TryChoose(
            out TimedAudioClip timedClip,
            out float selectedVolume,
            out float selectedPitch,
            out float selectedTail)
        {
            timedClip = default;
            selectedVolume = sesSeviyesi;
            selectedPitch = 1f;
            selectedTail = yumuşakBitişSüresi;

            if (sesKlipleri == null || sesKlipleri.Length == 0)
            {
                return false;
            }

            int startIndex = UnityEngine.Random.Range(0, sesKlipleri.Length);
            for (int offset = 0; offset < sesKlipleri.Length; offset++)
            {
                TimedAudioClip candidate = sesKlipleri[(startIndex + offset) % sesKlipleri.Length];
                if (candidate.Clip == null)
                {
                    continue;                }

                timedClip = candidate;
                selectedPitch = UnityEngine.Random.Range(
                    Mathf.Min(enDüşükPerde, enYüksekPerde),
                    Mathf.Max(enDüşükPerde, enYüksekPerde));
                return true;
            }
            return false;
        }
    }

    internal sealed class AudioVoicePool
    {
        private readonly MonoBehaviour _owner;
        private readonly AudioSource[] _voices;
        private readonly int[] _voiceGenerations;
        private int _nextVoice;

        internal AudioVoicePool(MonoBehaviour owner, int voiceCount)
        {
            _owner = owner;
            _voices = new AudioSource[Mathf.Clamp(voiceCount, 1, 8)];
            _voiceGenerations = new int[_voices.Length];

            for (int index = 0; index < _voices.Length; index++)
            {
                AudioSource source = owner.gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.dopplerLevel = 0f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 5f;
                source.maxDistance = 32f;
                _voices[index] = source;
            }
        }

        internal AudioClip LastPlayedClip { get; private set; }

        internal float PlayAligned(
            TimedAudioClipSet clipSet,
            float targetPeakDelay,
            float actionDuration,
            float spatialBlend)
        {
            if (clipSet == null || !clipSet.TryChoose(
                    out TimedAudioClip timedClip,
                    out float sesSeviyesi,
                    out float pitch,
                    out float yumuşakBitişSüresi))
            {
                return 0f;
            }

            AudioClip sesKlibi = timedClip.Clip;
            float safePitch = Mathf.Max(0.01f, pitch);
            float peakTime = Mathf.Clamp(timedClip.PeakTimeSeconds, 0f, Mathf.Max(0f, sesKlibi.length - 0.01f));
            float targetDelay = Mathf.Max(0f, targetPeakDelay);
            float untrimmedPeakDelay = peakTime / safePitch;
            float playbackDelay = Mathf.Max(0f, targetDelay - untrimmedPeakDelay);
            float startOffset = untrimmedPeakDelay > targetDelay
                ? peakTime - targetDelay * safePitch
                : 0f;
            startOffset = Mathf.Clamp(startOffset, 0f, Mathf.Max(0f, sesKlibi.length - 0.01f));

            float playbackWindow = Mathf.Max(0.05f, actionDuration + yumuşakBitişSüresi - playbackDelay);
            float remainingClipDuration = Mathf.Max(0f, (sesKlibi.length - startOffset) / safePitch);
            float audibleDuration = Mathf.Min(playbackWindow, remainingClipDuration);
            float fadeDuration = Mathf.Min(Mathf.Max(0.04f, yumuşakBitişSüresi), audibleDuration);

            LastPlayedClip = sesKlibi;
            _owner.StartCoroutine(PlayRoutine(
                sesKlibi,
                sesSeviyesi,
                pitch,
                Mathf.Clamp01(spatialBlend),
                playbackDelay,                startOffset,
                audibleDuration,
                fadeDuration));

            return playbackDelay + audibleDuration;
        }

        internal void StopAll()
        {
            _owner.StopAllCoroutines();
            for (int index = 0; index < _voices.Length; index++)
            {
                _voiceGenerations[index]++;
                _voices[index].Stop();
                _voices[index].clip = null;
            }
        }
        private IEnumerator PlayRoutine(
            AudioClip sesKlibi,
            float sesSeviyesi,
            float pitch,
            float spatialBlend,
            float playbackDelay,            float startOffset,
            float audibleDuration,
            float fadeDuration)
        {
            if (playbackDelay > 0f)
            {
                yield return new WaitForSeconds(playbackDelay);
            }

            int voiceIndex = _nextVoice;
            _nextVoice = (_nextVoice + 1) % _voices.Length;
            int generation = ++_voiceGenerations[voiceIndex];
            AudioSource source = _voices[voiceIndex];

            source.Stop();
            source.clip = sesKlibi;
            source.volume = Mathf.Clamp01(sesSeviyesi);
            source.pitch = pitch;
            source.spatialBlend = spatialBlend;
            source.time = startOffset;source.Play();

            float elapsed = 0f;
            float fadeStartsAt = Mathf.Max(0f, audibleDuration - fadeDuration);
            while (elapsed < audibleDuration)
            {
                if (_voiceGenerations[voiceIndex] != generation)
                {
                    yield break;
                }

                float fade = elapsed <= fadeStartsAt || fadeDuration <= 0f
                    ? 1f
                    : 1f - Mathf.Clamp01((elapsed - fadeStartsAt) / fadeDuration);
                source.volume = Mathf.Clamp01(sesSeviyesi * fade);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (_voiceGenerations[voiceIndex] == generation)
            {
                source.Stop();
                source.clip = null;
            }
        }
    }}