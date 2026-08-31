using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Vfx
{
    [DisallowMultipleComponent]
    public sealed class RuntimeVfxPool : MonoBehaviour
    {
        private const int MaximumIdlePerPrefab = 10;
        private static RuntimeVfxPool _instance;
        private readonly Dictionary<int, Stack<PooledVfxInstance>> _idle = new();
        private readonly HashSet<PooledVfxInstance> _active = new();

        public static int CreatedInstanceCount { get; private set; }
        public static int ActiveInstanceCount => _instance != null ? _instance._active.Count : 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            CreatedInstanceCount = 0;
        }

        public static void Play(GameObject prefab, Vector3 position, float scale, int sortingOrder, float lifetime)
        {
            if (prefab == null) return;
            Instance.Rent(prefab, position, scale, sortingOrder, lifetime);
        }

        private static RuntimeVfxPool Instance
        {
            get
            {
                if (_instance != null) return _instance;
                GameObject root = new("RuntimeVfxPool");
                DontDestroyOnLoad(root);
                _instance = root.AddComponent<RuntimeVfxPool>();
                return _instance;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Rent(GameObject prefab, Vector3 position, float scale, int sortingOrder, float lifetime)
        {
            int key = prefab.GetInstanceID();
            if (!_idle.TryGetValue(key, out Stack<PooledVfxInstance> stack))
            {
                stack = new Stack<PooledVfxInstance>();
                _idle[key] = stack;
            }

            PooledVfxInstance item = stack.Count > 0 ? stack.Pop() : Create(prefab, key);
            _active.Add(item);
            item.Play(position, scale, sortingOrder);
            StartCoroutine(ReturnAfter(item, Mathf.Max(.1f, lifetime)));
        }

        private PooledVfxInstance Create(GameObject prefab, int key)
        {
            GameObject effect = Instantiate(prefab, transform);
            effect.name = prefab.name + "_Runtime";
            PooledVfxInstance item = effect.AddComponent<PooledVfxInstance>();
            item.Initialize(key);
            CreatedInstanceCount++;
            return item;
        }

        private IEnumerator ReturnAfter(PooledVfxInstance item, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            Return(item);
        }

        private void Return(PooledVfxInstance item)
        {
            if (item == null || !_active.Remove(item)) return;
            item.Stop();
            if (!_idle.TryGetValue(item.PrefabKey, out Stack<PooledVfxInstance> stack))
            {
                stack = new Stack<PooledVfxInstance>();
                _idle[item.PrefabKey] = stack;
            }

            if (stack.Count >= MaximumIdlePerPrefab)
            {
                Destroy(item.gameObject);
                return;
            }
            stack.Push(item);
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene _, LoadSceneMode __)
        {
            if (_active.Count == 0) return;
            PooledVfxInstance[] snapshot = new PooledVfxInstance[_active.Count];
            _active.CopyTo(snapshot);
            foreach (PooledVfxInstance item in snapshot) Return(item);
        }
    }

    [DisallowMultipleComponent]
    public sealed class PooledVfxInstance : MonoBehaviour
    {
        private ParticleSystem[] _particles;
        private ParticleSystemRenderer[] _renderers;
        private Vector3 _baseScale;

        public int PrefabKey { get; private set; }

        public void Initialize(int prefabKey)
        {
            PrefabKey = prefabKey;
            _baseScale = transform.localScale;
            foreach (Light effectLight in GetComponentsInChildren<Light>(true)) effectLight.enabled = false;
            _particles = GetComponentsInChildren<ParticleSystem>(true);
            _renderers = GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (ParticleSystem particle in _particles)
            {
                ParticleSystem.MainModule main = particle.main;
                main.loop = false;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                main.maxParticles = Mathf.Min(main.maxParticles, 48);
            }
            gameObject.SetActive(false);
        }

        public void Play(Vector3 position, float scale, int sortingOrder)
        {
            transform.position = position;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.Scale(_baseScale, Vector3.one * Mathf.Max(.01f, scale));
            gameObject.SetActive(true);
            foreach (ParticleSystemRenderer renderer in _renderers) renderer.sortingOrder = sortingOrder;
            foreach (ParticleSystem particle in _particles)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particle.Play(true);
            }
        }

        public void Stop()
        {
            foreach (ParticleSystem particle in _particles)
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            gameObject.SetActive(false);
        }
    }
}
