using System;
using AncientTempleDefense.Allies;
using AncientTempleDefense.Economy;
using AncientTempleDefense.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace AncientTempleDefense.UI
{
    [DisallowMultipleComponent]
    public sealed class DefenseShopPanel : MonoBehaviour
    {
        [Header("Savunma Magazasi")]
        [SerializeField, InspectorName("Piksel Yazi Tipi")]
        private Font pikselYaziTipi;
        [SerializeField, InspectorName("Oyuncu")]
        private BlackKnightPlayerController oyuncu;
        [SerializeField, InspectorName("Coin Cuzdani")]
        private PlayerWallet coinCuzdani;
        [SerializeField, InspectorName("Martial Hero Prefabi")]
        private GameObject martialHeroPrefabi;
        [SerializeField, InspectorName("Hero Knight Prefabi")]
        private GameObject heroKnightPrefabi;
        [SerializeField, InspectorName("Dusuk Asker Prefablari")]
        private GameObject[] dusukAskerPrefabları;

        [Header("Fiyatlar ve Sinirlar")]
        [SerializeField, InspectorName("Asker Fiyati"), Min(0)]
        private int askerFiyati = 20;
        [SerializeField, InspectorName("Dusuk Asker Fiyati"), Min(0)]
        private int dusukAskerFiyati = 10;
        [SerializeField, InspectorName("Okcu Fiyati"), Min(0)]
        private int okcuFiyati = 50;
        [SerializeField, InspectorName("Buyucu Fiyati"), Min(0)]
        private int buyucuFiyati = 60;
        [SerializeField, InspectorName("En Fazla Dost"), Min(1)]
        private int enFazlaDost = 3;
        [SerializeField, InspectorName("Dost Dogma Y Konumu")]
        private float dostDogmaYKonumu = -3.25f;

        private GameObject _overlay;
        private Text _coinHud;
        private Text _shopCoinText;
        private Text _statusText;
        private Text _limitText;
        private Button _martialButton;
        private Button _knightButton;
        private Button _lowSoldierButton;
        private int _nextLowSoldier;
        private float _previousTimeScale = 1f;

        public bool IsOpen { get; private set; }
        public int SoldierPrice => askerFiyati;
        public int LowSoldierPrice => dusukAskerFiyati;
        public int ArcherPrice => okcuFiyati;
        public int MagePrice => buyucuFiyati;
        public int MaximumAllies => enFazlaDost;

        private void Awake()
        {
            BuildInterface();
        }

        private void OnEnable()
        {
            if (coinCuzdani != null)
            {
                coinCuzdani.CoinChanged += OnCoinChanged;
            }
            Refresh();
        }

        private void OnDisable()
        {
            if (coinCuzdani != null)
            {
                coinCuzdani.CoinChanged -= OnCoinChanged;
            }
            RestoreTimeScale();
        }

        public void ShowShop(int completedWave)
        {
            BuildInterface();
            _previousTimeScale = Mathf.Approximately(Time.timeScale, 0f) ? 1f : Time.timeScale;
            Time.timeScale = 0f;
            IsOpen = true;
            _overlay.SetActive(true);
            _statusText.text = "WAVE " + completedWave + " TAMAMLANDI";
            Refresh();
        }

        public bool TryBuy(int allyIndex)
        {
            bool lowSoldier = allyIndex == 2;
            GameObject prefab = lowSoldier
                ? NextLowSoldierPrefab()
                : allyIndex == 0 ? martialHeroPrefabi : heroKnightPrefabi;
            if (!IsOpen || prefab == null || coinCuzdani == null || oyuncu == null)
            {
                return false;
            }

            if (FriendlyDefender.AliveCount >= enFazlaDost)
            {
                _statusText.text = "AYNI ANDA EN FAZLA " + enFazlaDost + " DOST OLABILIR";
                Refresh();
                return false;
            }

            int price = lowSoldier ? dusukAskerFiyati : askerFiyati;
            if (!coinCuzdani.Harca(price))
            {
                _statusText.text = "YETERLI COIN YOK";
                Refresh();
                return false;
            }

            int slot = FriendlyDefender.AliveCount;
            float side = (slot & 1) == 0 ? -1f : 1f;
            float distance = 1.4f + (slot / 2) * 1.1f;
            Vector3 spawn = oyuncu.transform.position;
            spawn.x += side * distance;
            GameObject dost = Instantiate(prefab, spawn, Quaternion.identity);
            AlignDostWithPlayerGround(dost);
            _statusText.text = lowSoldier
                ? "DUSUK ASKER SAVASA KATILDI"
                : allyIndex == 0 ? "MARTIAL HERO SAVASA KATILDI" : "HERO KNIGHT SAVASA KATILDI";
            Refresh();
            return true;
        }

        private void AlignDostWithPlayerGround(GameObject dost)
        {
            if (dost == null)
            {
                return;
            }

            Collider2D oyuncuCollider = oyuncu != null ? oyuncu.GetComponent<Collider2D>() : null;
            Collider2D dostCollider = dost.GetComponent<Collider2D>();
            float hedefZeminY = oyuncuCollider != null
                ? oyuncuCollider.bounds.min.y
                : dostDogmaYKonumu;

            Vector3 position = dost.transform.position;
            if (dostCollider != null)
            {
                position.y += hedefZeminY - dostCollider.bounds.min.y;
            }
            else
            {
                position.y = hedefZeminY;
            }

            dost.transform.position = position;
        }

        public void CloseShop()
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;
            _overlay.SetActive(false);
            RestoreTimeScale();
        }

        private void BuildInterface()
        {
            if (_overlay != null)
            {
                return;
            }

            EnsureEventSystem();
            GameObject canvasObject = new("DefenseShopCanvas");
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform coinPanel = CreatePanel(canvasObject.transform, "CoinHUD", new Color(0.03f, 0.02f, 0.04f, 0.9f));
            SetAnchors(coinPanel, new Vector2(0.82f, 0.925f), new Vector2(0.985f, 0.985f));
            _coinHud = CreateText(coinPanel, "CoinText", "COIN 0", 30, TextAnchor.MiddleCenter, new Color(1f, 0.72f, 0.18f));
            SetAnchors(_coinHud.rectTransform, Vector2.zero, Vector2.one);
            _coinHud.resizeTextForBestFit = true;
            _coinHud.resizeTextMinSize = 18;
            _coinHud.resizeTextMaxSize = 30;

            _overlay = new GameObject("DefenseShopOverlay", typeof(RectTransform), typeof(Image));
            _overlay.transform.SetParent(canvasObject.transform, false);
            RectTransform overlayRect = (RectTransform)_overlay.transform;
            SetAnchors(overlayRect, Vector2.zero, Vector2.one);
            _overlay.GetComponent<Image>().color = new Color(0.012f, 0.008f, 0.022f, 0.95f);

            Text title = CreateText(overlayRect, "Title", "TAPINAK SAVUNMA MAGAZASI", 52, TextAnchor.MiddleCenter, new Color(1f, 0.58f, 0.20f));
            SetAnchors(title.rectTransform, new Vector2(0.15f, 0.82f), new Vector2(0.85f, 0.94f));

            _shopCoinText = CreateText(overlayRect, "ShopCoin", "COIN 0", 31, TextAnchor.MiddleCenter, new Color(1f, 0.78f, 0.25f));
            SetAnchors(_shopCoinText.rectTransform, new Vector2(0.37f, 0.75f), new Vector2(0.63f, 0.82f));

            RectTransform cards = CreatePanel(overlayRect, "Cards", Color.clear);
            SetAnchors(cards, new Vector2(0.18f, 0.27f), new Vector2(0.82f, 0.72f));
            HorizontalLayoutGroup layout = cards.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 35f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;
            _lowSoldierButton = CreateAllyCard(cards, "DUSUK ASKER", "UCUZ VE HAFIF", 2, dusukAskerFiyati);
            _martialButton = CreateAllyCard(cards, "MARTIAL HERO", "YAKIN DOVUSCU", 0, askerFiyati);
            _knightButton = CreateAllyCard(cards, "HERO KNIGHT", "ZIRHLI SAVASCI", 1, askerFiyati);

            _statusText = CreateText(overlayRect, "Status", "", 25, TextAnchor.MiddleCenter, new Color(0.84f, 0.78f, 0.70f));
            SetAnchors(_statusText.rectTransform, new Vector2(0.2f, 0.18f), new Vector2(0.8f, 0.26f));
            _limitText = CreateText(overlayRect, "Limit", "", 22, TextAnchor.MiddleCenter, new Color(0.65f, 0.78f, 0.86f));
            SetAnchors(_limitText.rectTransform, new Vector2(0.30f, 0.12f), new Vector2(0.70f, 0.19f));

            Button close = CreateButton(overlayRect, "DEVAM ET", CloseShop);
            RectTransform closeRect = (RectTransform)close.transform;
            SetAnchors(closeRect, new Vector2(0.39f, 0.035f), new Vector2(0.61f, 0.11f));

            _overlay.SetActive(false);
            Refresh();
        }

        private Button CreateAllyCard(Transform parent, string title, string description, int index, int priceValue)
        {
            RectTransform card = CreatePanel(parent, title, new Color(0.075f, 0.052f, 0.085f, 1f));
            VerticalLayoutGroup vertical = card.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(25, 25, 24, 24);
            vertical.spacing = 16f;
            vertical.childAlignment = TextAnchor.MiddleCenter;
            vertical.childControlHeight = true;
            vertical.childControlWidth = true;
            vertical.childForceExpandHeight = false;
            vertical.childForceExpandWidth = true;

            Text titleText = CreateText(card, "Name", title, 34, TextAnchor.MiddleCenter, Color.white);
            titleText.gameObject.AddComponent<LayoutElement>().preferredHeight = 70f;
            Text desc = CreateText(card, "Description", description, 24, TextAnchor.MiddleCenter, new Color(0.76f, 0.84f, 0.70f));
            desc.gameObject.AddComponent<LayoutElement>().preferredHeight = 80f;
            Text price = CreateText(card, "Price", priceValue + " COIN", 30, TextAnchor.MiddleCenter, new Color(1f, 0.68f, 0.20f));
            price.gameObject.AddComponent<LayoutElement>().preferredHeight = 60f;
            Button button = CreateButton(card, "SATIN AL", () => TryBuy(index));
            button.gameObject.AddComponent<LayoutElement>().preferredHeight = 68f;
            return button;
        }

        private Button CreateButton(Transform parent, string label, Action clicked)
        {
            GameObject go = new(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.24f, 0.12f, 0.08f, 1f);
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() => clicked());
            Text text = CreateText(go.transform, "Label", label, 28, TextAnchor.MiddleCenter, Color.white);
            SetAnchors(text.rectTransform, Vector2.zero, Vector2.one);
            return button;
        }

        private Text CreateText(Transform parent, string name, string value, int size, TextAnchor alignment, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = pikselYaziTipi;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private void OnCoinChanged(int coin)
        {
            Refresh();
        }

        private void Refresh()
        {
            int coin = coinCuzdani != null ? coinCuzdani.Coin : 0;
            if (_coinHud != null)
            {
                _coinHud.text = "COIN " + coin;
            }
            if (_shopCoinText != null)
            {
                _shopCoinText.text = "COIN " + coin;
            }

            int allies = FriendlyDefender.AliveCount;
            if (_limitText != null)
            {
                _limitText.text = "DOSTLAR " + allies + "/" + enFazlaDost + "   OKCU " + okcuFiyati + "   BUYUCU " + buyucuFiyati + " (YAKINDA)";
            }

            bool canBuy = IsOpen && allies < enFazlaDost && coin >= askerFiyati;
            if (_lowSoldierButton != null)
            {
                _lowSoldierButton.interactable = IsOpen && allies < enFazlaDost
                    && coin >= dusukAskerFiyati && HasLowSoldierPrefab();
            }
            if (_martialButton != null)
            {
                _martialButton.interactable = canBuy && martialHeroPrefabi != null;
            }
            if (_knightButton != null)
            {
                _knightButton.interactable = canBuy && heroKnightPrefabi != null;
            }
        }


        private bool HasLowSoldierPrefab()
        {
            if (dusukAskerPrefabları == null) return false;
            foreach (GameObject prefab in dusukAskerPrefabları)
            {
                if (prefab != null) return true;
            }
            return false;
        }

        private GameObject NextLowSoldierPrefab()
        {
            if (!HasLowSoldierPrefab()) return null;
            for (int offset = 0; offset < dusukAskerPrefabları.Length; offset++)
            {
                int index = (_nextLowSoldier + offset) % dusukAskerPrefabları.Length;
                if (dusukAskerPrefabları[index] == null) continue;
                _nextLowSoldier = index + 1;
                return dusukAskerPrefabları[index];
            }
            return null;
        }
        private void RestoreTimeScale()
        {
            if (IsOpen || Mathf.Approximately(Time.timeScale, 0f))
            {
                Time.timeScale = Mathf.Max(0.01f, _previousTimeScale);
            }
            IsOpen = false;
        }

        private static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return (RectTransform)go.transform;
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
