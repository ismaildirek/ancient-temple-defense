using AncientTempleDefense.Enemies;
using AncientTempleDefense.Player;
using AncientTempleDefense.Temple;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AncientTempleDefense.UI
{
    [DisallowMultipleComponent]
    public sealed class GameOverPanel : MonoBehaviour
    {
        [SerializeField, InspectorName("Pixel Yazı Tipi")] private Font pixelYazıTipi;
        [SerializeField, InspectorName("Ana Menü Sahnesi")] private string anaMenüSahnesi = "giris";

        private PlayerHealth _oyuncu;
        private TempleHealth _tapınak;
        private EnemyWaveSpawner _wave;
        private GameObject _panel;
        private Text _başlık;
        private Text _skor;
        private int _öldürülen;
        private bool _oyunBitti;

        public int KilledEnemies => _öldürülen;
        public bool IsVisible => _panel != null && _panel.activeSelf;
        public bool IsVictory { get; private set; }

        private void Awake()
        {
            Time.timeScale = 1f;
            _oyuncu = FindFirstObjectByType<PlayerHealth>();
            _tapınak = FindFirstObjectByType<TempleHealth>();
            _wave = FindFirstObjectByType<EnemyWaveSpawner>();
            BuildPanel();
        }

        private void OnEnable()
        {
            if (_oyuncu != null) _oyuncu.Died += FinishDefeat;
            if (_tapınak != null) _tapınak.Destroyed += FinishDefeat;
            EnemyCombatant.AnyEnemyDied += CountKill;
        }

        private void OnDisable()
        {
            if (_oyuncu != null) _oyuncu.Died -= FinishDefeat;
            if (_tapınak != null) _tapınak.Destroyed -= FinishDefeat;
            EnemyCombatant.AnyEnemyDied -= CountKill;
        }

        private void CountKill(EnemyCombatant enemy)
        {
            if (_oyunBitti) return;
            _öldürülen++;
            BossEnemyBrain boss = enemy != null ? enemy.GetComponent<BossEnemyBrain>() : null;
            if (boss != null && boss.BossTier == 4)
            {
                FinishGame(true);
            }
        }

        private void FinishDefeat()
        {
            FinishGame(false);
        }

        private void FinishGame(bool victory)
        {
            if (_oyunBitti) return;
            _oyunBitti = true;
            IsVictory = victory;
            int wave = _wave != null ? _wave.CurrentWave : 1;
            _başlık.text = victory ? "ZAFER" : "GAME OVER";
            _başlık.color = victory ? new Color(1f, .78f, .22f) : new Color(1f, .26f, .18f);
            _skor.text = $"ULASILAN WAVE: {wave}\nOLDURULEN CANAVAR: {_öldürülen}";
            _panel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void TryAgain()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        public void MainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(anaMenüSahnesi, LoadSceneMode.Single);
        }

        private void BuildPanel()
        {
            EnsureEventSystem();
            GameObject canvasRoot = new("GameOverCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasRoot.transform.SetParent(transform, false);
            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _panel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
            _panel.transform.SetParent(canvasRoot.transform, false);
            RectTransform panelRect = _panel.GetComponent<RectTransform>();
            SetAnchors(panelRect, Vector2.zero, Vector2.one);
            _panel.GetComponent<Image>().color = new Color(0.025f, 0.008f, 0.04f, 0.94f);

            RectTransform frame = CreatePanel(_panel.transform, "ScoreFrame", new Color(.10f, .045f, .08f, .98f));
            SetAnchors(frame, new Vector2(.27f, .18f), new Vector2(.73f, .82f));

            _başlık = CreateText(frame, "Title", "GAME OVER", 72, new Color(1f, .26f, .18f));
            SetAnchors(_başlık.rectTransform, new Vector2(.08f, .70f), new Vector2(.92f, .92f));
            _skor = CreateText(frame, "Score", string.Empty, 40, new Color(.96f, .82f, .58f));
            SetAnchors(_skor.rectTransform, new Vector2(.08f, .39f), new Vector2(.92f, .68f));

            Button retry = CreateButton(frame, "TRY AGAIN", TryAgain);
            SetAnchors((RectTransform)retry.transform, new Vector2(.09f, .11f), new Vector2(.47f, .29f));
            Button menu = CreateButton(frame, "MAIN MENU", MainMenu);
            SetAnchors((RectTransform)menu.transform, new Vector2(.53f, .11f), new Vector2(.91f, .29f));
            _panel.SetActive(false);
        }

        private Text CreateText(Transform parent, string name, string value, int size, Color color)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(Text));
            root.transform.SetParent(parent, false);
            Text text = root.GetComponent<Text>();
            text.font = pixelYazıTipi != null ? pixelYazıTipi : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value; text.fontSize = size; text.alignment = TextAnchor.MiddleCenter; text.color = color;
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 22; text.resizeTextMaxSize = size;
            return text;
        }

        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            GameObject root = new(label, typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            Image image = root.GetComponent<Image>(); image.color = new Color(.32f, .10f, .065f, 1f);
            Button button = root.GetComponent<Button>(); button.onClick.AddListener(action);
            Text text = CreateText(root.transform, "Label", label, 31, Color.white);
            SetAnchors(text.rectTransform, Vector2.zero, Vector2.one);
            return button;
        }

        private static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false); root.GetComponent<Image>().color = color;
            return (RectTransform)root.transform;
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
        }
    }
}