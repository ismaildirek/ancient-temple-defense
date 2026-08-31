using System;
using System.Collections.Generic;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.Progression;
using AncientTempleDefense.Temple;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
namespace AncientTempleDefense.UI
{
    [DisallowMultipleComponent]
    public sealed class WaveUpgradePanel : MonoBehaviour
    {
        [Header("Yükseltme Arayüzü")]
        [FormerlySerializedAs("pixelFont")]
        [SerializeField, InspectorName("Piksel Yazı Tipi")] private Font pikselYazıTipi;
        [FormerlySerializedAs("player")]
        [SerializeField, InspectorName("Oyuncu")] private BlackKnightPlayerController oyuncu;
        [FormerlySerializedAs("playerHealth")]
        [SerializeField, InspectorName("Oyuncu Canı")] private PlayerHealth oyuncuCanı;
        [FormerlySerializedAs("waveSpawner")]
        [SerializeField, InspectorName("Wave Sistemi")] private EnemyWaveSpawner waveSistemi;
        [SerializeField, InspectorName("Tapınak Canı")] private TempleHealth tapınakCanı;

        private readonly List<PlayerUpgradeCard> _visibleChoices = new();
        private GameObject _selectionOverlay;
private RectTransform _cardsRoot;
        private Text _waveText;
        private Text _healthText;
        private Text _templeHealthText;
        private Image _templeHealthFill;

        private float _timeScaleBeforeSelection = 1f;
        public bool IsChoosing { get; private set; }
        public Font PixelFont => pikselYazıTipi;
        public IReadOnlyList<PlayerUpgradeCard> VisibleChoices => _visibleChoices;

        private void Awake()
        {
            tapınakCanı ??= FindFirstObjectByType<TempleHealth>();
            BuildInterface();
        }

        private void OnEnable()
        {
            if (oyuncuCanı != null)
            {
                oyuncuCanı.HealthChanged += OnHealthChanged;
            }

            if (tapınakCanı != null)
            {
                tapınakCanı.HealthChanged += OnTempleHealthChanged;
            }

            RefreshPlayerStatus();
            RefreshTempleStatus();
        }

        private void OnDisable()
        {
            if (oyuncuCanı != null)
            {
                oyuncuCanı.HealthChanged -= OnHealthChanged;
            }

            if (tapınakCanı != null)
            {
                tapınakCanı.HealthChanged -= OnTempleHealthChanged;
            }

            RestoreTimeScale();
        }

        public void UpdateWaveStatus(int wave)
        {
            BuildInterface();
            _waveText.text = $"WAVE {wave}";
            RefreshPlayerStatus();
        }
        public void ShowChoices(int completedWave)
        {
            BuildInterface();
            _visibleChoices.Clear();
            _visibleChoices.AddRange(PlayerUpgradeCatalog.CreateChoices(completedWave));
            ClearChildren(_cardsRoot);

            foreach (PlayerUpgradeCard card in _visibleChoices)
            {
                CreateCard(card);
            }

            _timeScaleBeforeSelection = Mathf.Approximately(Time.timeScale, 0f) ? 1f : Time.timeScale;
            Time.timeScale = 0f;
            IsChoosing = true;
            _selectionOverlay.SetActive(true);
            RefreshPlayerStatus();
        }

        public void SelectChoice(int index)
        {
            if (!IsChoosing || index < 0 || index >= _visibleChoices.Count || oyuncu == null)
            {
                return;
            }

            oyuncu.ApplyUpgrade(_visibleChoices[index].Type);
            IsChoosing = false;
            _selectionOverlay.SetActive(false);
            RestoreTimeScale();
            RefreshPlayerStatus();
        }

        private void BuildInterface()
        {
            if (_selectionOverlay != null)
            {
                return;
            }

            EnsureEventSystem();
            GameObject canvasObject = new("WaveProgressionCanvas");
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform hud = CreatePanel(canvasObject.transform, "WaveHUD", new Color(0.025f, 0.02f, 0.035f, 0.90f));
            SetAnchors(hud, new Vector2(0.39f, 0.925f), new Vector2(0.61f, 0.985f), Vector2.zero, Vector2.zero);
            _waveText = CreateText(hud, "WaveText", "WAVE 1", 38, TextAnchor.MiddleCenter, new Color(1f, 0.58f, 0.22f));
            SetAnchors(_waveText.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));

            RectTransform healthPanel = CreatePanel(canvasObject.transform, "PlayerHUD", new Color(0.025f, 0.02f, 0.035f, 0.88f));
            SetAnchors(healthPanel, new Vector2(0.015f, 0.905f), new Vector2(0.34f, 0.985f), Vector2.zero, Vector2.zero);
            _healthText = CreateText(healthPanel, "HealthText", "CAN 100/100   ZIRH 0", 42, TextAnchor.MiddleLeft, new Color(0.94f, 0.35f, 0.27f));
            SetAnchors(_healthText.rectTransform, Vector2.zero, Vector2.one, new Vector2(20f, 0f), new Vector2(-12f, 0f));
            ConfigureBestFit(_healthText, 24, 42);

            RectTransform templePanel = CreatePanel(canvasObject.transform, "TempleHUD", new Color(0.025f, 0.02f, 0.035f, 0.90f));
            SetAnchors(templePanel, new Vector2(0.37f, 0.835f), new Vector2(0.63f, 0.915f), Vector2.zero, Vector2.zero);
            _templeHealthText = CreateText(templePanel, "TempleHealthText", "TAPINAK 2000/2000", 27, TextAnchor.MiddleCenter, new Color(0.90f, 0.72f, 0.34f));
            SetAnchors(_templeHealthText.rectTransform, new Vector2(0f, 0.43f), Vector2.one, new Vector2(12f, 0f), new Vector2(-12f, 0f));
            ConfigureBestFit(_templeHealthText, 18, 27);

            RectTransform templeBarBackground = CreatePanel(templePanel, "TempleHealthBarBackground", new Color(0.10f, 0.055f, 0.065f, 1f));
            SetAnchors(templeBarBackground, new Vector2(0.06f, 0.14f), new Vector2(0.94f, 0.43f), Vector2.zero, Vector2.zero);
            RectTransform templeBarFill = CreatePanel(templeBarBackground, "TempleHealthBarFill", new Color(0.84f, 0.48f, 0.15f, 1f));
            SetAnchors(templeBarFill, Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f));
            _templeHealthFill = templeBarFill.GetComponent<Image>();
            _templeHealthFill.type = Image.Type.Filled;
            _templeHealthFill.fillMethod = Image.FillMethod.Horizontal;
            _templeHealthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            _templeHealthFill.fillAmount = 1f;

            _selectionOverlay = new GameObject("UpgradeSelectionOverlay", typeof(RectTransform), typeof(Image));
            _selectionOverlay.transform.SetParent(canvasObject.transform, false);
            RectTransform overlayRect = (RectTransform)_selectionOverlay.transform;
            SetAnchors(overlayRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _selectionOverlay.GetComponent<Image>().color = new Color(0.015f, 0.01f, 0.025f, 0.94f);

            Text title = CreateText(overlayRect, "Title", "GÜÇLENME VAKTİ", 55, TextAnchor.MiddleCenter, new Color(1f, 0.60f, 0.24f));
            SetAnchors(title.rectTransform, new Vector2(0.18f, 0.80f), new Vector2(0.82f, 0.94f), Vector2.zero, Vector2.zero);
            Text subtitle = CreateText(overlayRect, "Subtitle", "BİR YÜKSELTME SEÇ", 26, TextAnchor.MiddleCenter, new Color(0.84f, 0.77f, 0.69f));
            SetAnchors(subtitle.rectTransform, new Vector2(0.20f, 0.74f), new Vector2(0.80f, 0.82f), Vector2.zero, Vector2.zero);

            GameObject cards = new("Cards", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cards.transform.SetParent(overlayRect, false);
            _cardsRoot = (RectTransform)cards.transform;
            SetAnchors(_cardsRoot, new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.72f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup horizontal = cards.GetComponent<HorizontalLayoutGroup>();
            horizontal.spacing = 28f;
            horizontal.padding = new RectOffset(12, 12, 12, 12);
            horizontal.childAlignment = TextAnchor.MiddleCenter;
            horizontal.childControlHeight = true;
            horizontal.childControlWidth = true;
            horizontal.childForceExpandHeight = true;
            horizontal.childForceExpandWidth = true;

            _selectionOverlay.SetActive(false);
            RefreshPlayerStatus();
        }

        private void CreateCard(PlayerUpgradeCard card)
        {
            GameObject cardObject = new(card.Title, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            cardObject.transform.SetParent(_cardsRoot, false);
            cardObject.GetComponent<Image>().color = new Color(0.075f, 0.055f, 0.085f, 0.98f);
            LayoutElement cardLayout = cardObject.GetComponent<LayoutElement>();
            cardLayout.minWidth = 380f;
            cardLayout.preferredWidth = 460f;
            VerticalLayoutGroup vertical = cardObject.GetComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(24, 24, 24, 24);
            vertical.spacing = 14f;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlHeight = true;
            vertical.childControlWidth = true;
            vertical.childForceExpandHeight = false;
            vertical.childForceExpandWidth = true;

            Text symbol = CreateText(cardObject.transform, "Symbol", card.Symbol, 48, TextAnchor.MiddleCenter, new Color(1f, 0.48f, 0.18f));
            symbol.gameObject.AddComponent<LayoutElement>().preferredHeight = 64f;
            Text title = CreateText(cardObject.transform, "CardTitle", card.Title, 34, TextAnchor.MiddleCenter, Color.white);
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 58f;
            Text description = CreateText(cardObject.transform, "Description", card.Description, 25, TextAnchor.MiddleCenter, new Color(0.78f, 0.85f, 0.71f));
            description.gameObject.AddComponent<LayoutElement>().preferredHeight = 115f;

            int choiceIndex = _visibleChoices.IndexOf(card);
            Button button = CreateButton(cardObject.transform, "SEÇ", () => SelectChoice(choiceIndex));
            button.gameObject.AddComponent<LayoutElement>().preferredHeight = 66f;
        }

        private Button CreateButton(Transform parent, string label, Action clicked)
        {
            GameObject buttonObject = new("SelectButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.22f, 0.13f, 0.10f, 1f);
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.48f, 0.25f, 0.12f, 1f);
            colors.pressedColor = new Color(0.70f, 0.34f, 0.14f, 1f);
            button.colors = colors;
            button.onClick.AddListener(() => clicked());
            Text text = CreateText(buttonObject.transform, "Label", label, 32, TextAnchor.MiddleCenter, Color.white);
            SetAnchors(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private Text CreateText(Transform parent, string name, string value, int size, TextAnchor alignment, Color color)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = pikselYazıTipi;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void ConfigureBestFit(Text text, int minimumSize, int maximumSize)
        {
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = minimumSize;
            text.resizeTextMaxSize = maximumSize;
        }
        private static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            return (RectTransform)panel.transform;
        }

        private static void SetAnchors(RectTransform rect, Vector2 minimum, Vector2 maximum, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = minimum;
            rect.anchorMax = maximum;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int index = parent.childCount - 1; index >= 0; index--)
            {
                Destroy(parent.GetChild(index).gameObject);
            }
        }

        private void RefreshPlayerStatus()
        {
            if (_healthText == null)
            {
                return;
            }

            if (oyuncuCanı != null)
            {
                _healthText.text = $"CAN {oyuncuCanı.CurrentHealth}/{oyuncuCanı.MaximumHealth}   ZIRH {oyuncuCanı.Armor}";
            }

        }

        private void OnHealthChanged(int currentHealth, int maximumHealth)
        {
            RefreshPlayerStatus();
        }

        private void RefreshTempleStatus()
        {
            if (_templeHealthText == null || _templeHealthFill == null)
            {
                return;
            }

            if (tapınakCanı == null)
            {
                _templeHealthText.text = "TAPINAK --";
                _templeHealthFill.fillAmount = 0f;
                return;
            }

            _templeHealthText.text = $"TAPINAK {tapınakCanı.CurrentHealth}/{tapınakCanı.MaximumHealth}";
            _templeHealthFill.fillAmount = (float)tapınakCanı.CurrentHealth / tapınakCanı.MaximumHealth;
            _templeHealthFill.color = tapınakCanı.DamageStage switch
            {
                TempleDamageStage.Hasarlı => new Color(0.95f, 0.42f, 0.12f, 1f),
                TempleDamageStage.Kritik => new Color(0.92f, 0.12f, 0.08f, 1f),
                _ => new Color(0.84f, 0.62f, 0.18f, 1f)
            };
        }

        private void OnTempleHealthChanged(int currentHealth, int maximumHealth, TempleDamageStage stage)
        {
            RefreshTempleStatus();
        }

        private void RestoreTimeScale()
        {
            if (IsChoosing || Mathf.Approximately(Time.timeScale, 0f))
            {
                Time.timeScale = Mathf.Max(0.01f, _timeScaleBeforeSelection);
            }

            IsChoosing = false;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(eventSystem);
        }
    }
}
