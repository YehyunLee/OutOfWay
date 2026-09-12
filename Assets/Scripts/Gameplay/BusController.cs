using UnityEngine;

namespace OutOfWay
{
    public class BusController : MonoBehaviour
    {
        public float StartSpeed = 9f;
        public float MaxSpeed = 28f;
        public float Acceleration = 0.28f;

        public float Speed { get; private set; }
        public bool Running { get; private set; }
        public bool Crashed { get; private set; }

        Transform[] _wheels;
        float _shake;
        Vector3 _originEuler;

        public void CollectWheels()
        {
            var spins = GetComponentsInChildren<SpinWithSpeed>();
            _wheels = new Transform[spins.Length];
            for (int i = 0; i < spins.Length; i++)
                _wheels[i] = spins[i].transform;
        }

        public void StartDriving()
        {
            Running = true;
            Crashed = false;
            Speed = StartSpeed;
            _originEuler = transform.eulerAngles;
        }

        public void Stop()
        {
            Running = false;
            Speed = 0f;
            SpinWithSpeed.Speed = 0f;
        }

        public void Crash()
        {
            if (Crashed) return;
            Crashed = true;
            Running = false;
            Speed = 0f;
            _shake = 0.55f;
            SpinWithSpeed.Speed = 0f;
        }

        void Update()
        {
            if (Running)
            {
                Speed = Mathf.Min(MaxSpeed, Speed + Acceleration * Time.deltaTime);
                transform.position += Vector3.forward * (Speed * Time.deltaTime);
                SpinWithSpeed.Speed = Speed;
            }

            if (_shake > 0f)
            {
                _shake -= Time.deltaTime;
                var jolt = Random.insideUnitSphere * _shake * 0.35f;
                transform.eulerAngles = _originEuler + new Vector3(jolt.x * 8f, jolt.y * 4f, jolt.z * 8f);
                if (_shake <= 0f) transform.eulerAngles = _originEuler;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            var obstacle = other.GetComponent<ObstacleController>();
            if (obstacle == null || obstacle.Cleared || obstacle.Parked) return;
            GameManager.Instance.HitObstacle(obstacle);
        }
    }

    public class CameraRig : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new(0f, 4.4f, -10.5f);
        public Vector3 LookAhead = new(0f, 1.4f, 16f);
        public float Follow = 8f;

        float _shake;

        public void Punch(float amount) => _shake = Mathf.Max(_shake, amount);

        void LateUpdate()
        {
            if (Target == null) return;
            var wanted = Target.position + Offset;
            transform.position = Vector3.Lerp(transform.position, wanted, 1f - Mathf.Exp(-Follow * Time.deltaTime));
            if (_shake > 0f)
            {
                transform.position += Random.insideUnitSphere * _shake;
                _shake -= Time.deltaTime * 3.5f;
            }

            transform.LookAt(Target.position + LookAhead);
        }
    }
}
