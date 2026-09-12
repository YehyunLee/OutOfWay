using UnityEngine;

namespace OutOfWay
{
    public class ObstacleSpawner : MonoBehaviour
    {
        public float FirstDelay = 2.4f;
        public float MinGap = 1.6f;
        public float MaxGap = 3.4f;

        BusController _bus;
        RhythmDirector _rhythm;
        Transform _holder;
        float _readyAt;
        ObstacleController _active;

        public ObstacleController Active => _active;

        public void Bind(BusController bus, RhythmDirector rhythm)
        {
            _bus = bus;
            _rhythm = rhythm;
            _holder = Build.Empty("Obstacles", transform);
            _readyAt = Time.time + FirstDelay;
        }

        public void ResetSpawner()
        {
            if (_holder != null)
            {
                for (int i = _holder.childCount - 1; i >= 0; i--)
                    Destroy(_holder.GetChild(i).gameObject);
            }

            _active = null;
            _readyAt = Time.time + FirstDelay;
        }

        public void OnResolved()
        {
            _active = null;
            _readyAt = Time.time + Random.Range(MinGap, MaxGap);
        }

        void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
            if (_rhythm.Busy || _active != null) return;
            if (Time.time < _readyAt) return;
            Spawn();
        }

        void Spawn()
        {
            var kind = RollKind();
            int seed = Random.Range(0, 9999);
            _active = VehicleFactory.MakeObstacle(kind, _holder, seed);

            float beats = 5.2f;
            float travel = Mathf.Max(38f, _bus.Speed * (beats * MusicConductor.Instance.BeatInterval) + 10f);
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
