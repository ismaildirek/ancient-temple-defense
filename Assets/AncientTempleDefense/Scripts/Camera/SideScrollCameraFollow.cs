using UnityEngine;
using UnityEngine.Serialization;

namespace AncientTempleDefense.CameraSystem
{
[DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class SideScrollCameraFollow : MonoBehaviour
    {
        [FormerlySerializedAs("target")]
        [SerializeField] private Transform hedef;
        [FormerlySerializedAs("horizontalBounds")]
        [SerializeField] private Vector2 yataySınırlar = new(-9.2f, 11.4f);
        [FormerlySerializedAs("smoothTime")]
        [SerializeField] private float yumuşatmaSüresi = 0.22f;
        [FormerlySerializedAs("horizontalDeadZone")]
        [SerializeField] private float yatayÖlüBölge = 1.2f;

        private float _velocity;

        private void LateUpdate()
        {
            if (hedef == null)
            {
                return;
            }

            Vector3 position = transform.position;
            float delta = hedef.position.x - position.x;
            if (Mathf.Abs(delta) <= yatayÖlüBölge)
            {
                return;
            }

            float desiredX = hedef.position.x - Mathf.Sign(delta) * yatayÖlüBölge;
            desiredX = Mathf.Clamp(desiredX, yataySınırlar.x, yataySınırlar.y);
            position.x = Mathf.SmoothDamp(position.x, desiredX, ref _velocity, yumuşatmaSüresi);
            transform.position = position;
        }
    }}
