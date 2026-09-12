using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace OutOfWay
{
    [DefaultExecutionOrder(-100)]
#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Designer assets — drag from Assets/Art")]
        public GameObject StreetAndBuildings;
        public GameObject Buildings;
        public GameObject Street;
        public GameObject Bus;
        public GameObject StreetLamp;

        [Header("Environment Props & Characters")]
        public GameObject Tree;
        public GameObject HumanFemale;
        public GameObject HumanMale;

        [Header("Music")]
        public float Bpm = 130f;
        public bool MetronomeClicks;
        public AudioClip BackgroundBed;

        [Header("Tempo Tracks (130, 140, 150 BPM)")]
        public AudioClip Music130;
        public AudioClip Metronome130;
        public AudioClip Music140;
        public AudioClip Metronome140;
        public AudioClip Music150;
        public AudioClip Metronome150;

        [Header("Word Sound Effects (Optional inspector overrides)")]
        public AudioClip WordGet;
        public AudioClip WordOut;
        public AudioClip WordOf;
        public AudioClip WordThe;
        public AudioClip WordWay;

        [Header("SFX (Optional inspector overrides)")]
        public AudioClip HonkSound;

        [Tooltip("Chant rhythms. Leave empty to use the built-in placeholder set.")]
        public PhraseLibrary Phrases;

        [Header("Camera Tuning")]
        public Vector3 CameraOffset = new(0f, 5.8f, -1.8f);
        public Vector3 CameraLookAhead = new(0f, 1.0f, 24f);

        [Header("Run")]
        [Tooltip("Seconds between the crash and the run restarting on its own.")]
        public float RestartDelay = 3f;

        [Tooltip("Multiplies every pattern's hit window. Raise it to make the whole game more forgiving.")]
        [Range(0.25f, 4f)] public float ToleranceScale = 1f;

        const string PreviewName = "DesignerStreet";
        bool _playBooted;
        CameraRig _rig;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<GameBootstrap>() != null) return;
            var root = new GameObject("OutOfWay");
            root.AddComponent<GameBootstrap>();
        }

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                BootPlay();
                return;
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += ShowDesignerStreet;
#endif
        }

        void Awake()
        {
            if (Application.isPlaying)
                BootPlay();
        }

        void Update()
        {
            if (_rig != null)
            {
                _rig.Offset = CameraOffset;
                _rig.LookAhead = CameraLookAhead;
            }
        }

        void BootPlay()
        {
            if (!Application.isPlaying || _playBooted) return;
            _playBooted = true;

            Look.ClearCache();
            Look.Init();
            StyleWorld();

#if UNITY_EDITOR
            if (Tree == null)
                Tree = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/SM_Tree_01.fbx");
            if (HumanFemale == null)
                HumanFemale = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/SM_HumanFemale/SM_HumanFemale.fbx");
            if (HumanMale == null)
                HumanMale = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/SM_HumanMale/SM_HumanMale.fbx");
#endif

            if (Tree == null)
                Tree = Resources.Load<GameObject>("Models/SM_Tree_01");
            if (HumanFemale == null)
                HumanFemale = Resources.Load<GameObject>("Models/SM_HumanFemale");
            if (HumanMale == null)
                HumanMale = Resources.Load<GameObject>("Models/SM_HumanMale");

            CityKit.Bind(StreetAndBuildings, Buildings, Street, StreetLamp, Tree, HumanFemale, HumanMale);

            var preview = transform.Find(PreviewName);
            if (preview != null) preview.gameObject.SetActive(false);

            var city = Build.Empty("City", transform).gameObject.AddComponent<EndlessCity>();
            var bus = VehicleFactory.MakeBus(transform, Bus).GetComponent<BusController>();
            city.Generate(bus.transform);

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.tag = "MainCamera";
            }

            cam.enabled = true;
            cam.transform.position = bus.transform.position + CameraOffset;
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.2f;
            cam.farClipPlane = 180f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Look.Sky;
            var urpCam = cam.GetUniversalAdditionalCameraData();
            if (urpCam != null)
                urpCam.renderPostProcessing = true;
            var rig = cam.GetComponent<CameraRig>() ?? cam.gameObject.AddComponent<CameraRig>();
            rig.Target = bus.transform;
            rig.Offset = CameraOffset;
            rig.LookAhead = CameraLookAhead;
            _rig = rig;

#if UNITY_EDITOR
            if (HonkSound == null)
                HonkSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/honk.mp3");
#endif
            if (HonkSound == null)
                HonkSound = Resources.Load<AudioClip>("Audio/honk") ?? Resources.Load<AudioClip>("Audio/Honk");

            var audio = ProceduralAudio.Create(transform, HonkSound);
            var music = Build.Empty("Music", transform).gameObject.AddComponent<MusicConductor>();
            music.Bpm = Bpm;
            music.MetronomeClicks = MetronomeClicks;
            music.BackgroundBed = BackgroundBed;
            if (Music130 != null || Music140 != null || Music150 != null)
            {
                music.Tiers = new[]
                {
                    new TempoTier(130, Music130, Metronome130),
                    new TempoTier(140, Music140 != null ? Music140 : Music130, Metronome140 != null ? Metronome140 : Metronome130),
                    new TempoTier(150, Music150 != null ? Music150 : Music130, Metronome150 != null ? Metronome150 : Metronome130)
                };
            }
            if (WordGet != null || WordOut != null || WordOf != null || WordThe != null || WordWay != null)
            {
                music.WordClips = new[] { WordGet, WordOut, WordOf, WordThe, WordWay };
            }
            music.Music = music.gameObject.AddComponent<AudioSource>();
            music.MetronomeSource = music.gameObject.AddComponent<AudioSource>();
            music.PhraseSource = music.gameObject.AddComponent<AudioSource>();
            music.PhraseSource.playOnAwake = false;
            music.PhraseSource.spatialBlend = 0f;
            music.SetupBed();

            var rhythm = gameObject.AddComponent<RhythmDirector>();
            rhythm.ToleranceScale = ToleranceScale;
            rhythm.Bind(music, Phrases != null ? Phrases : PhraseLibrary.CreateDefault());

            var spawner = gameObject.AddComponent<ObstacleSpawner>();
            spawner.Bind(bus, rhythm, music);

            var ui = GameUI.Create(transform);

            var game = gameObject.AddComponent<GameManager>();
            game.Bus = bus;
            game.Rig = rig;
            game.Rhythm = rhythm;
            game.Spawner = spawner;
            game.UI = ui;
            game.Music = music;
            game.RestartDelay = RestartDelay;
            game.Wire();

            audio.PlayEngine(false);
        }

        void ShowDesignerStreet()
        {
            if (this == null || Application.isPlaying) return;
            if (StreetAndBuildings == null && Street == null) return;
            if (transform.Find(PreviewName) != null) return;

            Look.ClearCache();
            Look.Init();

#if UNITY_EDITOR
            if (Tree == null)
                Tree = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/SM_Tree_01.fbx");
            if (HumanFemale == null)
                HumanFemale = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/SM_HumanFemale/SM_HumanFemale.fbx");
            if (HumanMale == null)
                HumanMale = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/SM_HumanMale/SM_HumanMale.fbx");
#endif

            if (Tree == null)
                Tree = Resources.Load<GameObject>("Models/SM_Tree_01");
            if (HumanFemale == null)
                HumanFemale = Resources.Load<GameObject>("Models/SM_HumanFemale");
            if (HumanMale == null)
                HumanMale = Resources.Load<GameObject>("Models/SM_HumanMale");

            CityKit.Bind(StreetAndBuildings, Buildings, Street, StreetLamp, Tree, HumanFemale, HumanMale);
            if (!CityKit.Ready) return;

            var holder = new GameObject(PreviewName).transform;
            holder.SetParent(transform, false);
            CityKit.Place(holder);
        }

        static void StyleWorld()
        {
            RenderSettings.fog = false;
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.92f, 0.88f, 0.80f);

            var sun = FindAnyObjectByType<Light>();
            if (sun != null)
            {
                sun.color = new Color(1f, 0.95f, 0.82f);
                sun.intensity = 1.6f;
                sun.transform.rotation = Quaternion.Euler(42f, -20f, 0f);
            }
        }
    }
}
