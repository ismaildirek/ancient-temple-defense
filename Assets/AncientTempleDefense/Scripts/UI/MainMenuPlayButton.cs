using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class MainMenuPlayButton : MonoBehaviour
    {
        [SerializeField, InspectorName("Ana Kamera")]
        private Camera anaKamera;
        [SerializeField, InspectorName("Acilacak Sahne")]
        private string acilacakSahne = "Map";
        [Header("Dikkat Ceken Hareket")]
        [SerializeField, InspectorName("Yukari Asagi Hareket"), Min(0f)]
        private float hareketMesafesi = 0.055f;
        [SerializeField, InspectorName("Hareket Hizi"), Min(0f)]
        private float hareketHizi = 2.2f;
        [SerializeField, InspectorName("Nefes Alma Olcegi"), Range(0f, 0.15f)]
        private float nefesOlcegi = 0.025f;

        private Collider2D _hitArea;
        private bool _sahneYukleniyor;
        private Vector3 _baslangicKonumu;
        private Vector3 _baslangicOlcegi;

        private void Awake()
        {
            _hitArea = GetComponent<Collider2D>();
            anaKamera ??= Camera.main;
            _baslangicKonumu = transform.localPosition;
            _baslangicOlcegi = transform.localScale;
        }

        private void Update()
        {
            AnimateButton();
            if (_sahneYukleniyor || !TryGetPressPosition(out Vector2 screenPosition))
            {
                return;
            }

            Camera activeCamera = anaKamera != null ? anaKamera : Camera.main;
            if (activeCamera == null)
            {
                return;
            }

            Vector2 worldPosition = activeCamera.ScreenToWorldPoint(screenPosition);
            if (_hitArea.OverlapPoint(worldPosition))
            {
                TryStartGame();
            }
        }

        private void AnimateButton()
        {
            float wave = Mathf.Sin(Time.unscaledTime * hareketHizi);
            transform.localPosition = _baslangicKonumu + Vector3.up * (wave * hareketMesafesi);
            transform.localScale = _baslangicOlcegi * (1f + Mathf.Abs(wave) * nefesOlcegi);
        }

        public bool TryStartGame()
        {
            if (_sahneYukleniyor || string.IsNullOrWhiteSpace(acilacakSahne))
            {
                return false;
            }

            _sahneYukleniyor = true;
            SceneManager.LoadScene(acilacakSahne, LoadSceneMode.Single);
            return true;
        }

        private static bool TryGetPressPosition(out Vector2 screenPosition)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }
    }
}
