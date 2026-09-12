using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace OutOfWay
{
    public class GameUI : MonoBehaviour
    {
        static readonly Color Ink = new(0.10f, 0.09f, 0.08f);
        static readonly Color Paper = new(0.96f, 0.93f, 0.86f);
        static readonly Color Mustard = new(0.93f, 0.74f, 0.16f);
        static readonly Color Teal = new(0.10f, 0.38f, 0.42f);
        static readonly Color Stamp = new(0.78f, 0.12f, 0.14f);

        Font _font;
        CanvasGroup _title;
        CanvasGroup _hud;
        CanvasGroup _fail;
        Text _score;
        Text _speed;
        Text _lyric;
        Text _banner;
        Text _failReason;
        Text _failScore;
        Image[] _pips;
        Image _honkPip;
        Image _honkFlash;
        Button _honkButton;
        float _flash;
        float _bannerUntil;

        public static GameUI Create(Transform parent)
        {
            var canvasGo = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(parent, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var es = new GameObject("EventSystem");
            es.transform.SetParent(parent, false);
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();

            var ui = canvasGo.AddComponent<GameUI>();
            ui.Build();
            return ui;
        }

        void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Helvetica", "Verdana" }, 64);

            _title = Panel("Title", transform, new Color(0.05f, 0.06f, 0.07f, 0.22f));
            Label(_title.transform, "OUT OF THE WAY", 118, Mustard, new Vector2(0, 160), 1400, 160, FontStyle.Bold);
            Label(_title.transform, "BUS DRIVER  ·  RHYTHM", 36, Paper, new Vector2(0, 52), 900, 50, FontStyle.Normal);
            Label(_title.transform, "They chant  GET  OUT  OF  THE WAY  —  honk the next beat.", 28, new Color(1f, 1f, 1f, 0.8f), new Vector2(0, -20), 1100, 40, FontStyle.Italic);
            Label(_title.transform, "HONK  /  SPACE  TO  DRIVE", 30, Mustard, new Vector2(0, -160), 800, 40, FontStyle.Bold);

            _hud = Panel("HUD", transform, Color.clear);
            _score = Label(_hud.transform, "CLEARED  0", 34, Paper, new Vector2(0, 480), 600, 48, FontStyle.Bold);
            _speed = Label(_hud.transform, "12 MPH", 24, new Color(1f, 1f, 1f, 0.7f), new Vector2(0, 440), 400, 36, FontStyle.Normal);
            _banner = Label(_hud.transform, "", 28, Mustard, new Vector2(0, 390), 700, 40, FontStyle.Bold);
            _lyric = Label(_hud.transform, "", 92, Paper, new Vector2(0, 220), 1200, 120, FontStyle.Bold);

            var pipRow = new GameObject("Pips", typeof(RectTransform)).transform;
            pipRow.SetParent(_hud.transform, false);
            var row = pipRow.GetComponent<RectTransform>();
            Stretch(row);
            row.anchoredPosition = new Vector2(0, 110);

            _pips = new Image[4];
            float[] xs = { -180, -60, 60, 180 };
            for (int i = 0; i < 4; i++)
                _pips[i] = Pip(pipRow, xs[i], 22f, Teal);

            _honkPip = Pip(pipRow, 0, -40, 34f, Stamp);
            _honkPip.enabled = false;

            _honkFlash = Image(_hud.transform, "Flash", Color.clear, Vector2.zero, new Vector2(1920, 1080));
            _honkFlash.raycastTarget = false;

            _honkButton = CircleButton(_hud.transform, "HONK", new Vector2(0, -420), 210);
            _honkButton.onClick.AddListener(() => GameManager.Instance.Honk());

            _fail = Panel("Fail", transform, new Color(0.04f, 0.03f, 0.03f, 0.35f));
            Label(_fail.transform, "DRIVER'S LICENSE", 36, Paper, new Vector2(0, 150), 800, 48, FontStyle.Bold);
            Label(_fail.transform, "REVOKED", 110, Stamp, new Vector2(0, 40), 1100, 140, FontStyle.Bold);
            _failReason = Label(_fail.transform, "", 30, Paper, new Vector2(0, -70), 800, 40, FontStyle.Normal);
            _failScore = Label(_fail.transform, "", 28, Mustard, new Vector2(0, -130), 800, 40, FontStyle.Bold);
            var retry = RectButton(_fail.transform, "TRY AGAIN", new Vector2(0, -230), new Vector2(320, 72));
            retry.onClick.AddListener(() => GameManager.Instance.Retry());

            ShowTitle();
        }

        public void ShowTitle()
        {
            _title.alpha = 1f;
            _title.blocksRaycasts = true;
            _hud.alpha = 0f;
            _hud.blocksRaycasts = false;
            _fail.alpha = 0f;
            _fail.blocksRaycasts = false;
        }

        public void ShowPlaying()
        {
            _title.alpha = 0f;
            _title.blocksRaycasts = false;
            _hud.alpha = 1f;
            _hud.blocksRaycasts = true;
            _fail.alpha = 0f;
            _fail.blocksRaycasts = false;
            ResetPips();
            _lyric.text = "";
            _banner.text = "";
        }

        public void ShowCue(int index, string word)
        {
            _lyric.text = word;
            _lyric.color = Paper;
            _honkPip.enabled = false;
            for (int i = 0; i < _pips.Length; i++)
                _pips[i].color = i <= index ? Mustard : new Color(Teal.r, Teal.g, Teal.b, 0.35f);

            if (GameManager.Instance.Spawner.Active != null)
                _banner.text = GameManager.Instance.Spawner.Active.Label;
        }

        public void ShowHonkWindow()
        {
            _lyric.text = "HONK";
            _lyric.color = Stamp;
            _honkPip.enabled = true;
            _honkPip.color = Stamp;
        }

        public void ShowClear(int score, bool perfect)
        {
            _score.text = $"CLEARED  {score}";
            _banner.text = perfect ? "PERFECT  —  THEY MOVED" : "THEY MOVED";
            _bannerUntil = Time.time + 1.1f;
            _lyric.text = "";
            ResetPips();
        }

        public void ShowMiss(string reason)
        {
            _banner.text = reason;
            _lyric.color = Stamp;
        }

        public void ShowRevoked(string reason, int score, int best)
        {
            _hud.blocksRaycasts = false;
            _fail.alpha = 1f;
            _fail.blocksRaycasts = true;
            _failReason.text = reason;
            _failScore.text = $"CLEARED  {score}    BEST  {best}";
        }

        public void FlashHonk() => _flash = 0.18f;

        void ResetPips()
        {
            for (int i = 0; i < _pips.Length; i++)
                _pips[i].color = new Color(Teal.r, Teal.g, Teal.b, 0.35f);
            _honkPip.enabled = false;
        }

        void Update()
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.State == GameState.Title)
            {
                if (AnyStartInput()) GameManager.Instance.Honk();
                return;
            }

            if (PressedHonk())
                GameManager.Instance.Honk();

            if (GameManager.Instance.IsPlaying && GameManager.Instance.Bus != null)
                _speed.text = $"{Mathf.RoundToInt(GameManager.Instance.Bus.Speed * 2.4f)} MPH";

            if (_flash > 0f)
            {
                _flash -= Time.deltaTime;
                _honkFlash.color = new Color(1f, 0.92f, 0.45f, _flash * 0.35f);
            }
            else _honkFlash.color = Color.clear;

            if (_bannerUntil > 0f && Time.time > _bannerUntil && GameManager.Instance.IsPlaying)
            {
                _banner.text = "";
                _bannerUntil = 0f;
            }
        }

        static bool AnyStartInput()
        {
            if (Keyboard.current != null &&
                (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                return true;
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                return true;
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }

        static bool PressedHonk()
        {
            if (Keyboard.current != null &&
                (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                return true;
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                return true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return false;
                return true;
            }

            return false;
        }

        CanvasGroup Panel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            Stretch(go.GetComponent<RectTransform>());
            go.GetComponent<Image>().color = color;
            go.GetComponent<Image>().raycastTarget = color.a > 0.05f;
            var group = go.GetComponent<CanvasGroup>();
            group.interactable = true;
            return group;
        }

        Text Label(Transform parent, string text, int size, Color color, Vector2 pos, float w, float h, FontStyle style)
        {
            var go = new GameObject(text == "" ? "Label" : text, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = pos;
            var label = go.GetComponent<Text>();
            label.font = _font;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        Image Pip(Transform parent, float x, float size, Color color) => Pip(parent, x, 0f, size, color);

        Image Pip(Transform parent, float x, float y, float size, Color color)
        {
            var img = Image(parent, "Pip", color, new Vector2(x, y), new Vector2(size, size));
            img.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            return img;
        }

        Image Image(Transform parent, string name, Color color, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        Button CircleButton(Transform parent, string caption, Vector2 pos, float size)
        {
            var go = new GameObject(caption, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = pos;
            var img = go.GetComponent<Image>();
            img.color = Mustard;
            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.88f, 0.35f);
            colors.pressedColor = new Color(0.75f, 0.55f, 0.08f);
            button.colors = colors;
            Label(go.transform, caption, 36, Ink, Vector2.zero, size, 50, FontStyle.Bold);
            return button;
        }

        Button RectButton(Transform parent, string caption, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(caption, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            go.GetComponent<Image>().color = Mustard;
            var button = go.GetComponent<Button>();
            Label(go.transform, caption, 28, Ink, Vector2.zero, size.x, size.y, FontStyle.Bold);
            return button;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
