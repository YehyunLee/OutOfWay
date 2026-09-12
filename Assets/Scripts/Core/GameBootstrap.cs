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

        [Header("Music")]
        public float Bpm = 100f;
        public bool MetronomeClicks;
        public AudioClip BackgroundBed;

        [Tooltip("Chant rhythms. Leave empty to use the built-in placeholder set.")]
        public PhraseLibrary Phrases;

        [Header("Run")]
        [Tooltip("Seconds between the crash and the run restarting on its own.")]
        public float RestartDelay = 3f;

        [Tooltip("Multiplies every pattern's hit window. Raise it to make the whole game more forgiving.")]
        [Range(0.25f, 4f)] public float ToleranceScale = 1f;

        const string PreviewName = "DesignerStreet";
        bool _playBooted;

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

        void BootPlay()
        {
            if (!Application.isPlaying || _playBooted) return;
            _playBooted = true;

            Look.Init();
            StyleWorld();
            CityKit.Bind(StreetAndBuildings, Buildings, Street);

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
            cam.transform.position = bus.transform.position + new Vector3(0f, 4.4f, -10.5f);
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

            var audio = ProceduralAudio.Create(transform);
            var music = Build.Empty("Music", transform).gameObject.AddComponent<MusicConductor>();
            music.Bpm = Bpm;
            music.MetronomeClicks = MetronomeClicks;
            music.BackgroundBed = BackgroundBed;
            music.Music = music.gameObject.AddComponent<AudioSource>();
            music.PhraseSource = music.gameObject.AddComponent<AudioSource>();
            music.PhraseSource.playOnAwake = false;
            music.PhraseSource.spatialBlend = 0f;
            music.SetupBed();

            var rhythm = gameObject.AddComponent<RhythmDirector>();
            rhythm.ToleranceScale = ToleranceScale;
            rhythm.Bind(music, Phrases != null ? Phrases : PhraseLibrary.CreateDefault());

            var spawner = gameObject.AddComponent<ObstacleSpawner>();
            spawner.Bind(bus, rhythm);

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

            Look.Init();
            CityKit.Bind(StreetAndBuildings, Buildings, Street);
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
