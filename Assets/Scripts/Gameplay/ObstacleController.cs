using UnityEngine;

namespace OutOfWay
{
    public class ObstacleController : MonoBehaviour
    {
        public ObstacleKind Kind;
        public bool Cleared;
        public bool Parked;
        public MeshRenderer SirenA;
        public MeshRenderer SirenB;

        Vector3 _dodgeTarget;
        bool _dodging;
        float _siren;

        public string Label => Kind switch
        {
            ObstacleKind.Bicycle => "BICYCLE AHEAD",
            ObstacleKind.Ambulance => "AMBULANCE AHEAD",
            _ => "CAR AHEAD"
        };

        public void CollectWheels() { }

        public void Dodge()
        {
            if (Cleared) return;
            Cleared = true;
            _dodging = true;
            float side = Random.value < 0.5f ? -1f : 1f;
            _dodgeTarget = transform.position + new Vector3(side * 5.4f, 0f, 3.5f);
        }

        void Update()
        {
            if (_dodging)
            {
                transform.position = Vector3.Lerp(transform.position, _dodgeTarget, 1f - Mathf.Exp(-7f * Time.deltaTime));
                var look = _dodgeTarget - transform.position;
                if (look.sqrMagnitude > 0.05f)
                {
                    var yaw = Quaternion.LookRotation(new Vector3(look.x, 0f, look.z + 2f));
                    transform.rotation = Quaternion.Slerp(transform.rotation, yaw, Time.deltaTime * 6f);
                }
            }

            if (Kind == ObstacleKind.Ambulance && SirenA != null && SirenB != null)
            {
                _siren += Time.deltaTime * 8f;
                bool blink = Mathf.PingPong(_siren, 1f) > 0.5f;
                SirenA.enabled = blink;
                SirenB.enabled = !blink;
            }

            var bus = GameManager.Instance != null ? GameManager.Instance.Bus : null;
            if (bus != null && transform.position.z < bus.transform.position.z - 16f)
                Destroy(gameObject);
        }
    }
}
