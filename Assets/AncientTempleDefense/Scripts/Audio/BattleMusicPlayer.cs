using UnityEngine;
using UnityEngine.Serialization;

namespace AncientTempleDefense.Audio
{
[DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class BattleMusicPlayer : MonoBehaviour
    {
        [FormerlySerializedAs("musicClip")]
        [SerializeField] private AudioClip müzikKlibi;
        [FormerlySerializedAs("volume")]
        [SerializeField, Range(0f, 1f)] private float sesSeviyesi = 0.24f;

        private static BattleMusicPlayer _instance;
        private AudioSource _source;
        private bool _isDuplicate;

        public AudioClip MusicClip => müzikKlibi;
        public AudioSource Source => _source;
        public bool IsConfigured => müzikKlibi != null && _source != null && _source.loop;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                _isDuplicate = true;
                Destroy(gameObject);
                return;
            }

            _instance = this;
            transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject);

            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.spatialBlend = 0f;
            _source.dopplerLevel = 0f;
            _source.volume = sesSeviyesi;
            _source.clip = müzikKlibi;
        }
        private void OnEnable()
        {
            if (!_isDuplicate && _source != null && müzikKlibi != null && !_source.isPlaying)
            {
                _source.Play();
            }        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
