using UnityEngine;

namespace OutOfWay
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Music — drop the track here when it's ready")]
        public float Bpm = 100f;
        public bool MetronomeClicks = true;
        public AudioClip GetOutOfTheWayPhrase;

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

            cam.transform.position = bus.transform.position + new Vector3(0f, 5.2f, -10.5f);
            cam.fieldOfView = 58f;
            cam.farClipPlane = 280f;
            var rig = cam.GetComponent<CameraRig>() ?? cam.gameObject.AddComponent<CameraRig>();
            rig.Target = bus.transform;

            var audio = ProceduralAudio.Create(transform);
            var music = Build.Empty("Music", transform).gameObject.AddComponent<MusicConductor>();
            music.Bpm = Bpm;
            music.MetronomeClicks = MetronomeClicks;
            music.GetOutOfTheWayPhrase = GetOutOfTheWayPhrase;
            music.Music = music.gameObject.AddComponent<AudioSource>();
            music.Music.playOnAwake = false;

            var rhythm = gameObject.AddComponent<RhythmDirector>();
            rhythm.Bind(music);

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
            game.Wire();

            audio.PlayEngine(false);
        }

        static void StyleWorld()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = Look.Fog;
            RenderSettings.fogDensity = 0.011f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.74f, 0.86f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.52f, 0.45f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.24f, 0.18f);

            var sun = FindAnyObjectByType<Light>();
            if (sun != null)
            {
                sun.color = new Color(1f, 0.93f, 0.78f);
                sun.intensity = 1.35f;
                sun.transform.rotation = Quaternion.Euler(38f, -25f, 0f);
            }
        }
    }
}
