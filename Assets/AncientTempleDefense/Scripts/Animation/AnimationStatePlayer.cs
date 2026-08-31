using System.Collections.Generic;
using UnityEngine;

namespace AncientTempleDefense.Animation
{
    public sealed class AnimationStatePlayer
    {
        private readonly Animator _animator;
        private readonly Dictionary<string, float> _clipLengths = new();
        private int _currentStateHash;

        public AnimationStatePlayer(Animator animator)
        {
            _animator = animator;

            if (_animator == null || _animator.runtimeAnimatorController == null)
            {
                return;
            }

            foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
            {
                if (clip != null)
                {
                    _clipLengths[clip.name] = clip.length;
                }
            }
        }

        public void Play(string stateName, bool restart = false)
        {
            if (_animator == null || string.IsNullOrEmpty(stateName))
            {
                return;
            }

            int stateHash = Animator.StringToHash(stateName);
            if (!restart && stateHash == _currentStateHash)
            {
                return;
            }

            if (!_animator.HasState(0, stateHash))
            {
                Debug.LogWarning($"Animator state bulunamadi: {stateName}", _animator);
                return;
            }

            _animator.Play(stateHash, 0, 0f);
            _currentStateHash = stateHash;
        }

        public float Duration(string clipName, float fallback = 0.4f)
        {
            return _clipLengths.TryGetValue(clipName, out float duration)
                ? Mathf.Max(0.05f, duration)
                : fallback;
        }
    }
}
