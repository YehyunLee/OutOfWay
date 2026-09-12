using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace OutOfWay
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Music — drop the track here when it's ready")]
        public float Bpm = 100f;
        public bool MetronomeClicks = true;

        [Tooltip("Chant rhythms. Leave empty to use the built-in placeholder set.")]
        public PhraseLibrary Phrases;

        [Header("Run")]
        [Tooltip("Seconds between the crash and the run restarting on its own.")]
        public float RestartDelay = 3f;

        [Tooltip("Multiplies every pattern's hit window. Raise it to make the whole game more forgiving.")]
        [Range(0.25f, 4f)] public float ToleranceScale = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<GameBootstrap>() != null) return;
            var root = new GameObject("OutOfWay");
            root.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            Look.Init();
            StyleWorld();

            var city = Build.Empty("City", transform).gameObject.AddComponent<EndlessCity>();
            var bus = VehicleFactory.MakeBus(transform).GetComponent<BusController>();
            city.Generate(bus.transform);

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.tag = "MainCamera";
            }

            cam.transform.position = bus.transform.position + new Vector3(0f, 3.5f, -8.2f);
            cam.fieldOfView = 62f;
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
