using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace OutOfWay
{
    public class GameUI : MonoBehaviour
    {
        static readonly Color Ink = new(0.12f, 0.10f, 0.08f);
        static readonly Color Paper = new(0.98f, 0.95f, 0.88f);
        static readonly Color Mustard = new(0.96f, 0.76f, 0.12f);
        static readonly Color Teal = new(0.10f, 0.38f, 0.42f);
        static readonly Color Stamp = new(0.82f, 0.12f, 0.14f);
        static readonly Color ChipBg = new(0.06f, 0.07f, 0.08f, 0.62f);

        static readonly string[] PipWords = { "GET", "OUT", "OF", "WAY" };

        Font _font;
        Sprite _circle;
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
        Text[] _pipLabels;
        Image _honkPip;
        Image _honkFlash;
        RectTransform _honkRt;
        Image _honkImage;
        bool _honkArmed;
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
            _circle = CircleSprite(128);

            _title = Panel("Title", transform, new Color(0.02f, 0.03f, 0.04f, 0.18f));
            Image(_title.transform, "TopBar", new Color(0.05f, 0.05f, 0.06f, 0.72f), new Vector2(0, 430), new Vector2(1920, 220));
            Label(_title.transform, "OUT OF THE WAY", 86, Mustard, new Vector2(0, 470), 1400, 100, FontStyle.Bold);
            Label(_title.transform, "BUS DRIVER RHYTHM", 26, Paper, new Vector2(0, 400), 800, 36, FontStyle.Normal);
            Image(_title.transform, "BottomBar", new Color(0.05f, 0.05f, 0.06f, 0.78f), new Vector2(0, -430), new Vector2(1920, 220));
            Label(_title.transform, "They chant GET / OUT / OF / THE WAY.  Honk the next beat.", 26, new Color(1f, 1f, 1f, 0.82f), new Vector2(0, -390), 1400, 40, FontStyle.Italic);
            Label(_title.transform, "SPACE  OR  HONK  TO  DRIVE", 32, Mustard, new Vector2(0, -450), 900, 44, FontStyle.Bold);

            _hud = Panel("HUD", transform, Color.clear);
            _hud.GetComponent<Image>().raycastTarget = false;

            Chip(_hud.transform, new Vector2(48, -36), new Vector2(0, 1), new Vector2(0, 1), new Vector2(280, 84));
            _score = Label(_hud.transform, "CLEARED  0", 28, Paper, Vector2.zero, 240, 40, FontStyle.Bold);
            Anchor(_score.rectTransform, new Vector2(0, 1), new Vector2(160, -48));

            Chip(_hud.transform, new Vector2(-48, -36), new Vector2(1, 1), new Vector2(1, 1), new Vector2(220, 84));
            _speed = Label(_hud.transform, "0 MPH", 28, Mustard, Vector2.zero, 180, 40, FontStyle.Bold);
            Anchor(_speed.rectTransform, new Vector2(1, 1), new Vector2(-140, -48));

            _banner = Label(_hud.transform, "", 26, Mustard, new Vector2(0, 330), 900, 40, FontStyle.Bold);
            _lyric = Label(_hud.transform, "", 64, Paper, new Vector2(0, 250), 900, 80, FontStyle.Bold);

            var track = new GameObject("BeatTrack", typeof(RectTransform)).transform;
            track.SetParent(_hud.transform, false);
            var trackRt = track.GetComponent<RectTransform>();
            trackRt.anchorMin = trackRt.anchorMax = new Vector2(0.5f, 0f);
            trackRt.pivot = new Vector2(0.5f, 0f);
            trackRt.sizeDelta = new Vector2(760, 140);
            trackRt.anchoredPosition = new Vector2(0, 250);

            _pips = new Image[4];
            _pipLabels = new Text[4];
            float[] xs = { -270, -90, 90, 270 };
            for (int i = 0; i < 4; i++)
            {
                _pips[i] = Circle(track, xs[i], 36, 42, new Color(1f, 1f, 1f, 0.16f));
                _pipLabels[i] = Label(track, PipWords[i], 16, new Color(1f, 1f, 1f, 0.45f), new Vector2(xs[i], -8), 80, 24, FontStyle.Bold);
            }

            _honkPip = Circle(track, 0, 88, 56, new Color(Stamp.r, Stamp.g, Stamp.b, 0.15f));
            _honkPip.enabled = false;

            _honkFlash = Image(_hud.transform, "Flash", Color.clear, Vector2.zero, new Vector2(1920, 1080));
            _honkFlash.raycastTarget = false;

            var honk = CircleButton(_hud.transform, "HONK", new Vector2(0, 96), 168);
            _honkRt = honk.GetComponent<RectTransform>();
            _honkImage = honk.GetComponent<Image>();
            honk.onClick.AddListener(() => GameManager.Instance.Honk());
            Label(_hud.transform, "SPACE", 18, new Color(1f, 1f, 1f, 0.55f), new Vector2(0, 18), 200, 24, FontStyle.Bold);

            _fail = Panel("Fail", transform, new Color(0.02f, 0.02f, 0.03f, 0.45f));
            Chip(_fail.transform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760, 520));
            Label(_fail.transform, "DRIVER'S LICENSE", 28, Paper, new Vector2(0, 150), 600, 40, FontStyle.Bold);
            Label(_fail.transform, "REVOKED", 84, Stamp, new Vector2(0, 55), 700, 110, FontStyle.Bold);
            _failReason = Label(_fail.transform, "", 26, Paper, new Vector2(0, -20), 600, 36, FontStyle.Normal);
            _failScore = Label(_fail.transform, "", 24, Mustard, new Vector2(0, -70), 600, 36, FontStyle.Bold);
            var retry = RectButton(_fail.transform, "TRY AGAIN", new Vector2(0, -170), new Vector2(280, 64));
            retry.onClick.AddListener(() => GameManager.Instance.Retry());
            Label(_fail.transform, "SPACE  TO  RETRY", 18, new Color(1f, 1f, 1f, 0.5f), new Vector2(0, -230), 300, 24, FontStyle.Normal);

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
            _score.text = "CLEARED  0";
            ResetPips();
            _lyric.text = "";
            _banner.text = "";
            _honkArmed = false;
            if (_honkRt != null) _honkRt.gameObject.SetActive(true);
        }

        public void ShowCue(int index, string word)
        {
            _lyric.text = word;
            _lyric.color = Paper;
            _honkArmed = false;
            _honkPip.enabled = false;
            for (int i = 0; i < _pips.Length; i++)
            {
                bool on = i <= index;
                _pips[i].color = on ? Mustard : new Color(1f, 1f, 1f, 0.16f);
                _pipLabels[i].color = on ? Ink : new Color(1f, 1f, 1f, 0.45f);
            }

            if (GameManager.Instance.Spawner.Active != null)
                _banner.text = GameManager.Instance.Spawner.Active.Label;
        }

        public void ShowHonkWindow()
        {
            _lyric.text = "HONK";
            _lyric.color = Stamp;
            _honkArmed = true;
            _honkPip.enabled = true;
            _honkPip.color = Stamp;
        }

        public void ShowClear(int score, bool perfect)
        {
            _score.text = $"CLEARED  {score}";
            _banner.text = perfect ? "PERFECT — THEY MOVED" : "THEY MOVED";
            _bannerUntil = Time.time + 1.1f;
            _lyric.text = "";
            _honkArmed = false;
            ResetPips();
        }

        public void ShowMiss(string reason)
        {
            _banner.text = reason;
            _lyric.color = Stamp;
            _honkArmed = false;
        }

        public void ShowRevoked(string reason, int score, int best)
        {
            _hud.blocksRaycasts = false;
            if (_honkRt != null) _honkRt.gameObject.SetActive(false);
            _fail.alpha = 1f;
            _fail.blocksRaycasts = true;
            _failReason.text = reason;
            _failScore.text = $"CLEARED  {score}     BEST  {best}";
        }

        public void FlashHonk() => _flash = 0.16f;

        void ResetPips()
        {
            for (int i = 0; i < _pips.Length; i++)
            {
                _pips[i].color = new Color(1f, 1f, 1f, 0.16f);
                _pipLabels[i].color = new Color(1f, 1f, 1f, 0.45f);
            }
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
                _honkFlash.color = new Color(1f, 0.92f, 0.45f, _flash * 0.28f);
            }
            else _honkFlash.color = Color.clear;

            if (_honkRt != null && _honkImage != null && _honkRt.gameObject.activeSelf)
            {
                float pulse = _honkArmed ? 1f + Mathf.PingPong(Time.time * 6f, 0.12f) : 1f;
                _honkRt.localScale = Vector3.one * pulse;
                _honkImage.color = _honkArmed ? Color.Lerp(Mustard, Stamp, Mathf.PingPong(Time.time * 4f, 1f)) : Mustard;
            }

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
            var go = new GameObject(string.IsNullOrEmpty(text) ? "Label" : text, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
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

        Image Circle(Transform parent, float x, float y, float size, Color color)
        {
            var img = Image(parent, "Pip", color, new Vector2(x, y), new Vector2(size, size));
            img.sprite = _circle;
            img.type = UnityEngine.UI.Image.Type.Simple;
            return img;
        }

        Button CircleButton(Transform parent, string caption, Vector2 pos, float size)
        {
            var go = new GameObject(caption, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = pos;
            var img = go.GetComponent<Image>();
            img.sprite = _circle;
            img.color = Mustard;
            img.type = UnityEngine.UI.Image.Type.Simple;
            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.88f, 0.35f);
            colors.pressedColor = new Color(0.75f, 0.55f, 0.08f);
            button.colors = colors;
            Label(go.transform, caption, 34, Ink, Vector2.zero, size, 50, FontStyle.Bold);
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
            Label(go.transform, caption, 26, Ink, Vector2.zero, size.x, size.y, FontStyle.Bold);
            return button;
        }

        void Chip(Transform parent, Vector2 pos, Vector2 min, Vector2 max, Vector2 size)
        {
            var img = Image(parent, "Chip", ChipBg, pos, size);
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = min;
            rt.pivot = min;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        static void Anchor(RectTransform rt, Vector2 anchor, Vector2 pos)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static Sprite CircleSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            float cx = (size - 1) * 0.5f;
            float r = size * 0.5f - 1.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cx));
                    float a = Mathf.Clamp01(r - d);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }
    }
}
