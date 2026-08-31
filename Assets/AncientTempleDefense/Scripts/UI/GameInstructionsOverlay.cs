using UnityEngine;
using UnityEngine.Serialization;

namespace AncientTempleDefense.UI
{
    public sealed class GameInstructionsOverlay : MonoBehaviour
    {
        [FormerlySerializedAs("pixelFont")]
        [SerializeField, InspectorName("Piksel Yazı Tipi")] private Font pikselYazıTipi;

        [Header("Yerleşim")]
        [SerializeField, InspectorName("Üst HUD Altı Boşluk"), Min(80f)] private float üstHudAltıBoşluk = 142f;
        [SerializeField, InspectorName("Panel Genişliği"), Min(300f)] private float panelGenişliği = 1060f;
        [SerializeField, InspectorName("Yazı Boyutu"), Range(10, 24)] private int yazıBoyutu = 16;

        private GUIStyle _style;

        public Rect CurrentPanelRect => CalculatePanelRect(Screen.width, Screen.height, üstHudAltıBoşluk, panelGenişliği);

        public static Rect CalculatePanelRect(int screenWidth, int screenHeight, float topOffset = 142f, float width = 1060f)
        {
            float scale = Mathf.Clamp(screenHeight / 1080f, 0.72f, 1.5f);
            float margin = 18f * scale;
            float panelWidth = Mathf.Max(0f, Mathf.Min(width * scale, screenWidth - margin * 2f));
            return new Rect(margin, topOffset * scale, panelWidth, 42f * scale);
        }

        private void OnGUI()
        {
            _style ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                font = pikselYazıTipi,
                normal = { textColor = new Color(1f, 0.82f, 0.54f) },
                padding = new RectOffset(12, 12, 8, 8)
            };

            float scale = Mathf.Clamp(Screen.height / 1080f, 0.72f, 1.5f);
            _style.font = pikselYazıTipi;
            _style.fontSize = Mathf.Max(10, Mathf.RoundToInt(yazıBoyutu * scale));

            const string controls = "A/D HAREKET   W ZIPLA   S TAKLA   1 KOMBO   2 AĞIR   3 SAVUNMA   4 ULTİ   Q SİLAH";
            GUI.Box(CurrentPanelRect, controls, _style);
        }
    }
}