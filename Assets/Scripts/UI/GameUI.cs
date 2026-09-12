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
        // Opaque: the tilted stats overlap, and a translucent fill would composite twice and darken.
        static readonly Color StatBg = new(0.45f, 0.47f, 0.50f, 1f);
        static readonly Color Green = new(0.26f, 0.84f, 0.36f);
        static readonly Color Dim = new(1f, 1f, 1f, 0.20f);

        static readonly string[] PhraseWords = { "Get", "Out", "Of", "The", "Way" };

        Font _font;
        Font _lyricFont;
        Sprite _circle;
        CanvasGroup _title;
        CanvasGroup _hud;
        CanvasGroup _fail;
        Text _score;
        Text _speed;
        Text _streak;
        Text _banner;
        Text _failReason;
        Text _failScore;
        Text[] _words;
        Text _failStamp;
        Image _honkFlash;
        RectTransform _honkRt;
        Image _honkImage;
        bool _honkArmed;
        float _flash;
        float _bannerUntil;
        float _streakPunchUntil;

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

            // The loaded scene may already carry one, and Unity errors on a second.
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.transform.SetParent(parent, false);
                es.AddComponent<EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
            }

            var ui = canvasGo.AddComponent<GameUI>();
            ui.Build();
            return ui;
        }

        void Build()
        {
            _font = Resources.Load<Font>("Fonts/BrownieStencil");
            if (_font == null) _font = Resources.Load<Font>("Fonts/ArchivoBlack-Regular");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Helvetica", "Verdana" }, 64);
            _lyricFont = Resources.Load<Font>("Fonts/BrownieStencil");
            if (_lyricFont == null) _lyricFont = _font;
            _circle = CircleSprite(128);

            _title = Panel("Title", transform, new Color(0.02f, 0.03f, 0.04f, 0.18f));
            Image(_title.transform, "TopBar", new Color(0.05f, 0.05f, 0.06f, 0.72f), new Vector2(0, 430), new Vector2(1920, 220));
            Label(_title.transform, "OUT OF THE WAY", 86, Mustard, new Vector2(0, 446), 1400, 100, FontStyle.Bold);
            Label(_title.transform, "BUS DRIVER RHYTHM", 26, Paper, new Vector2(0, 376), 800, 36, FontStyle.Normal);
            Image(_title.transform, "BottomBar", new Color(0.05f, 0.05f, 0.06f, 0.78f), new Vector2(0, -430), new Vector2(1920, 220));
            Label(_title.transform, "They chant Get Out Of The Way.  Honk it back — once per word, same rhythm.", 26, new Color(1f, 1f, 1f, 0.82f), new Vector2(0, -390), 1400, 40, FontStyle.Italic);
            Label(_title.transform, "SPACE  OR  HONK  TO  DRIVE", 32, Mustard, new Vector2(0, -450), 900, 44, FontStyle.Bold);

            _hud = Panel("HUD", transform, Color.clear);
            _hud.GetComponent<Image>().raycastTarget = false;

            // Hand-stamped look: each stat sits at its own angle rather than in a tidy column.
            _score = TiltedStat("0 Hit", 58, Paper, new Vector2(0, 1), new Vector2(220, -140), new Vector2(340, 112), -6.5f);
            _streak = TiltedStat("0 IN A ROW", 42, Mustard, new Vector2(0, 1), new Vector2(292, -254), new Vector2(380, 92), 7.5f);
            _speed = TiltedStat("0 MPH", 50, Mustard, new Vector2(1, 1), new Vector2(-196, -142), new Vector2(300, 104), 5f);

            _banner = Label(_hud.transform, "", 26, Mustard, new Vector2(0, 330), 900, 40, FontStyle.Bold);
            BuildPhraseRow();
            _failStamp = Label(_hud.transform, "", 96, Stamp, new Vector2(0, 140), 900, 120, FontStyle.Bold);
            _failStamp.font = _lyricFont;

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
            Label(_fail.transform, "RESTARTING  —  SPACE  TO  SKIP", 18, new Color(1f, 1f, 1f, 0.5f), new Vector2(0, -230), 420, 24, FontStyle.Normal);

            ShowTitle();
        }

        /// <summary>Chip and label in one rotated holder, so the backing tilts with the text.</summary>
        Text TiltedStat(string text, int size, Color color, Vector2 anchor, Vector2 pos, Vector2 chipSize, float angle)
        {
            var holder = new GameObject("Stat", typeof(RectTransform)).transform;
            holder.SetParent(_hud.transform, false);
            var rt = holder.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = chipSize;
            rt.anchoredPosition = pos;
            rt.localEulerAngles = new Vector3(0f, 0f, angle);

            Image(holder, "Chip", StatBg, Vector2.zero, chipSize);
            var label = Label(holder, text, size, color, Vector2.zero, chipSize.x - 24f, chipSize.y, FontStyle.Bold);
            label.font = _lyricFont;
            return label;
        }

        void BuildPhraseRow()
        {
            var row = new GameObject("Phrase", typeof(RectTransform), typeof(HorizontalLayoutGroup)).transform;
            row.SetParent(_hud.transform, false);
            var rowRt = row.GetComponent<RectTransform>();
            rowRt.anchorMin = rowRt.anchorMax = rowRt.pivot = new Vector2(0.5f, 0.5f);
            rowRt.sizeDelta = new Vector2(1600, 130);
            rowRt.anchoredPosition = new Vector2(0, 250);

            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            _words = new Text[PhraseWords.Length];
            for (int i = 0; i < PhraseWords.Length; i++)
            {
                var word = Label(row, PhraseWords[i], 84, Dim, Vector2.zero, 200, 110, FontStyle.Bold);
                word.font = _lyricFont;
                var fitter = word.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                _words[i] = word;
            }
        }

        void PaintPhrase(Color color)
        {
            for (int i = 0; i < _words.Length; i++)
                _words[i].color = color;
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
            _score.text = "0 Hit";
            _streak.text = "0 IN A ROW";
            PaintPhrase(Dim);
            _failStamp.text = "";
            _banner.text = "";
            _honkArmed = false;
            if (_honkRt != null) _honkRt.gameObject.SetActive(true);
        }

        /// <summary>A word of the chant just landed. Index 0 starts a fresh phrase.</summary>
        public void ShowCue(int index, string word)
        {
            if (index == 0)
            {
                PaintPhrase(Dim);
                _failStamp.text = "";
            }

            if (index >= 0 && index < _words.Length)
            {
                _words[index].text = word;
                _words[index].color = Paper;
            }

            _honkArmed = false;

            if (GameManager.Instance.Spawner.Active != null)
                _banner.text = GameManager.Instance.Spawner.Active.Label;
        }

        /// <summary>The chant is done — the player's turn to honk it back.</summary>
        public void ShowResponse()
        {
            PaintPhrase(Mustard);
            _banner.text = "HONK IT BACK";
            _bannerUntil = 0f;
            _honkArmed = true;
        }

        public void ShowHonkAccepted(int index)
        {
            if (index < 0 || index >= _words.Length) return;
            _words[index].color = Green;
        }

        public void ShowClear(int score, bool perfect)
        {
            _score.text = $"{score} Hit";
            _banner.text = perfect ? "PERFECT — THEY MOVED" : "THEY MOVED";
            _bannerUntil = Time.time + 1.1f;
            // Green holds until the next chant starts.
            PaintPhrase(Green);
            _honkArmed = false;
        }

        public void ShowMiss(string reason)
        {
            _banner.text = reason;
            PaintPhrase(Stamp);
            _failStamp.text = "FAIL!";
            _honkArmed = false;
        }

        public void ShowRevoked(string reason, int score, int best)
        {
            _hud.blocksRaycasts = false;
            if (_honkRt != null) _honkRt.gameObject.SetActive(false);
            _fail.alpha = 1f;
            _fail.blocksRaycasts = true;
            _failReason.text = reason;
            _failScore.text = $"{score} HIT     BEST  {best}";
        }

        public void FlashHonk() => _flash = 0.16f;

        /// <summary>Consecutive honks landed on rhythm, carried across phrases and reset on a miss.</summary>
        public void ShowStreak(int streak)
        {
            _streak.text = streak == 1 ? "1 IN A ROW" : $"{streak} IN A ROW";
            _streak.color = streak >= 10 ? Stamp : Mustard;
            _streakPunchUntil = Time.time + 0.22f;
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

            if (_streak != null)
            {
                float left = _streakPunchUntil - Time.time;
                float punch = left > 0f ? 1f + 0.35f * (left / 0.22f) : 1f;
                _streak.transform.localScale = Vector3.one * punch;
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
