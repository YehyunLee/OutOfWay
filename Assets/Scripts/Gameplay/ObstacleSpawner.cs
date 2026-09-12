using UnityEngine;

namespace OutOfWay
{
    /// <summary>
    /// Spawns roadway obstacles and coordinates phrase onset with the background music
    /// and metronome downbeat (Start Beat), ensuring the first sound ("GET") lands exactly
    /// on the start beat of a measure.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        public float FirstDelay = 2.4f;
        public float MinGap = 1.6f;
        public float MaxGap = 3.4f;

        BusController _bus;
        RhythmDirector _rhythm;
        MusicConductor _music;
        Transform _holder;
        float _readyAt;
        bool _pendingSpawn;
        float _pendingSince;
        ObstacleController _active;

        public ObstacleController Active => _active;

        public void Bind(BusController bus, RhythmDirector rhythm, MusicConductor music = null)
        {
            if (_music != null) _music.StartBeatTriggered -= OnStartBeat;

            _bus = bus;
            _rhythm = rhythm;
            _music = music != null ? music : MusicConductor.Instance;
            if (_music != null) _music.StartBeatTriggered += OnStartBeat;

            _holder = Build.Empty("Obstacles", transform);
            _readyAt = Time.time + FirstDelay;
            _pendingSpawn = false;
        }

        void OnDisable()
        {
            if (_music != null) _music.StartBeatTriggered -= OnStartBeat;
        }

        void OnDestroy()
        {
            if (_music != null) _music.StartBeatTriggered -= OnStartBeat;
        }

        public void ResetSpawner()
        {
            if (_holder != null)
            {
                for (int i = _holder.childCount - 1; i >= 0; i--)
                    Destroy(_holder.GetChild(i).gameObject);
            }

            _active = null;
            _pendingSpawn = false;
            _readyAt = Time.time + FirstDelay;
        }

        public void OnResolved()
        {
            _active = null;
            _pendingSpawn = false;
            _readyAt = Time.time + Random.Range(MinGap, MaxGap);
        }

        void OnStartBeat(int bar)
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
            if (_rhythm.Busy || _active != null) return;
            if (!_pendingSpawn) return;

            _pendingSpawn = false;
            Spawn();
        }

        void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
            if (_rhythm.Busy || _active != null) return;

            // Lazy-wire music conductor if not bound yet
            if (_music == null && MusicConductor.Instance != null)
            {
                _music = MusicConductor.Instance;
                _music.StartBeatTriggered += OnStartBeat;
            }

            // Arm spawner once the inter-obstacle gap delay has passed
            if (!_pendingSpawn)
            {
                if (Time.time < _readyAt) return;
                _pendingSpawn = true;
                _pendingSince = Time.time;
            }

            // Once armed, trigger on the Start Beat of the background music / metronome
            if (_pendingSpawn)
            {
                var conductor = _music != null ? _music : MusicConductor.Instance;
                bool isAudioPlaying = conductor != null && conductor.Music != null && conductor.Music.isPlaying;

                if (isAudioPlaying)
                {
                    // If OnStartBeat event didn't fire due to script execution order or frame hitch,
                    // catch it here on IsStartBeatThisFrame, or use safety timeout
                    if (conductor.IsStartBeatThisFrame || (Time.time - _pendingSince > 2.5f))
                    {
                        _pendingSpawn = false;
                        Spawn();
                    }
                }
                else
                {
                    // Fallback when audio is disabled or not playing
                    _pendingSpawn = false;
                    Spawn();
                }
            }
        }

        void Spawn()
        {
            var kind = RollKind();
            int seed = Random.Range(0, 9999);
            _active = VehicleFactory.MakeObstacle(kind, _holder, seed);

            // Place them so the bus arrives around the time the call-and-response resolves.
            var pattern = _rhythm.PeekNext();
            var conductor = _music != null ? _music : MusicConductor.Instance;
            float tempo = conductor != null ? Mathf.Max(0.1f, conductor.TempoScale) : 1f;
            float seconds = pattern != null ? pattern.TotalDuration / tempo : 5f;
            float travel = Mathf.Max(38f, _bus.Speed * seconds + 10f);
            _active.transform.position = new Vector3(0f, 0f, _bus.transform.position.z + travel);
            _rhythm.BeginPhrase();
        }

        static ObstacleKind RollKind()
        {
            float roll = Random.value;
            if (roll < 0.18f) return ObstacleKind.Ambulance;
            if (roll < 0.40f) return ObstacleKind.Bicycle;
            return ObstacleKind.Car;
        }
    }
}
