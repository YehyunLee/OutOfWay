using UnityEngine;

namespace OutOfWay
{
    public enum ObstacleKind
    {
        Car,
        Bicycle,
        Ambulance
    }

    public static class VehicleFactory
    {
        public static Transform MakeBus(Transform parent, GameObject prefab = null)
        {
            var root = Build.Empty("Bus", parent);
            root.position = Vector3.zero;

            if (prefab != null)
                AttachDesignerBus(root, prefab);
            else
                AttachPrimitiveBus(root);

            var b = CombinedBounds(root.gameObject);
            var hit = root.gameObject.AddComponent<BoxCollider>();
            hit.center = root.InverseTransformPoint(b.center);
            hit.size = Vector3.Max(b.size, new Vector3(1.5f, 1.5f, 3f));
            hit.isTrigger = true;

            var body = root.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            var bus = root.gameObject.AddComponent<BusController>();
            bus.CollectWheels();
            return root;
        }

        static void AttachDesignerBus(Transform root, GameObject prefab)
        {
            var visual = Object.Instantiate(prefab, root);
            visual.name = "SM_Bus";
            foreach (var cam in visual.GetComponentsInChildren<Camera>(true))
            {
                cam.enabled = false;
                if (cam.gameObject == visual) Object.DestroyImmediate(cam);
                else Object.DestroyImmediate(cam.gameObject);
            }

            foreach (var light in visual.GetComponentsInChildren<Light>(true))
            {
                if (light.gameObject == visual) Object.DestroyImmediate(light);
                else Object.DestroyImmediate(light.gameObject);
            }

            foreach (var col in visual.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(col);

            foreach (var t in visual.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name.ToLowerInvariant();
                if (n.Contains("clider") || n.Contains("collider"))
                    t.gameObject.SetActive(false);
            }

            // Maya bus faces +X. Game drives +Z.
            visual.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one;

            var b = CombinedBounds(visual);
            float length = Mathf.Max(0.01f, b.size.z);
            float scale = 7.0f / length;
            visual.transform.localScale = Vector3.one * scale;

            b = CombinedBounds(visual);
            visual.transform.position += new Vector3(-b.center.x, -b.min.y, -b.center.z);

            Look.UseImported(visual);
            foreach (var t in visual.GetComponentsInChildren<Transform>())
            {
                string n = t.name.ToLowerInvariant();
                if (n.Contains("clider") || n.Contains("collider"))
                    continue;
                if (n.Contains("wheel"))
                    t.gameObject.AddComponent<SpinWithSpeed>();
            }
        }

        static void AttachPrimitiveBus(Transform root)
        {
            Build.Box(root, "Body", new Vector3(0f, 1.35f, 0f), new Vector3(2.4f, 1.7f, 7.2f), Look.BusMat);
            Build.Box(root, "Stripe", new Vector3(0f, 1.15f, 0f), new Vector3(2.46f, 0.28f, 7.24f), Look.BusStripeMat);
            Build.Box(root, "Roof", new Vector3(0f, 2.28f, -0.1f), new Vector3(2.2f, 0.16f, 6.6f), Look.BusMat);
            Build.Box(root, "Windows", new Vector3(0f, 1.85f, 0.05f), new Vector3(2.42f, 0.55f, 6.4f), Look.GlassMat);
            Build.Box(root, "Windshield", new Vector3(0f, 1.7f, 3.45f), new Vector3(2.2f, 0.85f, 0.08f), Look.GlassMat);
            Build.Box(root, "Bumper", new Vector3(0f, 0.42f, 3.7f), new Vector3(2.5f, 0.28f, 0.22f), Look.ChromeMat);
            Build.Box(root, "RearBumper", new Vector3(0f, 0.42f, -3.7f), new Vector3(2.5f, 0.28f, 0.22f), Look.ChromeMat);
            Build.Box(root, "Sign", new Vector3(0f, 2.42f, 3.2f), new Vector3(1.6f, 0.28f, 0.12f), Look.DarkMat);
            Build.Box(root, "MirrorL", new Vector3(-1.38f, 1.7f, 3.1f), new Vector3(0.28f, 0.18f, 0.12f), Look.ChromeMat);
            Build.Box(root, "MirrorR", new Vector3(1.38f, 1.7f, 3.1f), new Vector3(0.28f, 0.18f, 0.12f), Look.ChromeMat);
            Build.Sphere(root, "HeadL", new Vector3(-0.75f, 0.72f, 3.68f), 0.28f, Look.HeadlightMat);
            Build.Sphere(root, "HeadR", new Vector3(0.75f, 0.72f, 3.68f), 0.28f, Look.HeadlightMat);
            Build.Box(root, "TailL", new Vector3(-0.85f, 0.85f, -3.68f), new Vector3(0.35f, 0.18f, 0.08f), Look.TaillightMat);
            Build.Box(root, "TailR", new Vector3(0.85f, 0.85f, -3.68f), new Vector3(0.35f, 0.18f, 0.08f), Look.TaillightMat);
            Build.Box(root, "Driver", new Vector3(-0.55f, 1.55f, 2.7f), new Vector3(0.4f, 0.55f, 0.28f), Look.DarkMat);
            Wheel(root, "WFL", new Vector3(-1.15f, 0.38f, 2.2f));
            Wheel(root, "WFR", new Vector3(1.15f, 0.38f, 2.2f));
            Wheel(root, "WML", new Vector3(-1.15f, 0.38f, -0.4f));
            Wheel(root, "WMR", new Vector3(1.15f, 0.38f, -0.4f));
            Wheel(root, "WRL", new Vector3(-1.15f, 0.38f, -2.4f));
            Wheel(root, "WRR", new Vector3(1.15f, 0.38f, -2.4f));
        }

        static Bounds CombinedBounds(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }

        public static ObstacleController MakeObstacle(ObstacleKind kind, Transform parent, int seed)
        {
            return kind switch
            {
                ObstacleKind.Bicycle => MakeBike(parent, seed, true),
                ObstacleKind.Ambulance => MakeAmbulance(parent),
                _ => MakeCar(parent, seed)
            };
        }

        static ObstacleController MakeCar(Transform parent, int seed)
        {
            var root = Build.Empty("Car", parent);
            root.localScale = Vector3.one * 1.55f;
            var paint = Look.Car(seed);
            Build.Box(root, "Body", new Vector3(0f, 0.62f, 0f), new Vector3(1.7f, 0.7f, 3.6f), paint);
            Build.Box(root, "Cabin", new Vector3(0f, 1.12f, -0.15f), new Vector3(1.55f, 0.5f, 1.8f), Look.GlassMat);
            Build.Box(root, "Hood", new Vector3(0f, 0.78f, 1.15f), new Vector3(1.6f, 0.18f, 1.1f), paint);
            Build.Sphere(root, "HeadL", new Vector3(-0.55f, 0.55f, 1.82f), 0.18f, Look.HeadlightMat);
            Build.Sphere(root, "HeadR", new Vector3(0.55f, 0.55f, 1.82f), 0.18f, Look.HeadlightMat);
            Build.Box(root, "TailL", new Vector3(-0.55f, 0.58f, -1.82f), new Vector3(0.28f, 0.12f, 0.06f), Look.TaillightMat);
            Build.Box(root, "TailR", new Vector3(0.55f, 0.58f, -1.82f), new Vector3(0.28f, 0.12f, 0.06f), Look.TaillightMat);
            Wheel(root, "WFL", new Vector3(-0.82f, 0.28f, 1.1f), 0.28f);
            Wheel(root, "WFR", new Vector3(0.82f, 0.28f, 1.1f), 0.28f);
            Wheel(root, "WRL", new Vector3(-0.82f, 0.28f, -1.15f), 0.28f);
            Wheel(root, "WRR", new Vector3(0.82f, 0.28f, -1.15f), 0.28f);
            return FinishObstacle(root, ObstacleKind.Car, new Vector3(1.8f, 1.3f, 3.8f));
        }

        static ObstacleController MakeAmbulance(Transform parent)
        {
            var root = Build.Empty("Ambulance", parent);
            root.localScale = Vector3.one * 1.5f;
            Build.Box(root, "Body", new Vector3(0f, 1.05f, 0f), new Vector3(2.0f, 1.6f, 4.4f), Look.AmbulanceMat);
            Build.Box(root, "Stripe", new Vector3(0f, 1.05f, 0f), new Vector3(2.06f, 0.28f, 4.44f), Look.AmbulanceRedMat);
            Build.Box(root, "Cab", new Vector3(0f, 1.15f, 1.55f), new Vector3(1.95f, 1.15f, 1.3f), Look.AmbulanceMat);
            Build.Box(root, "Glass", new Vector3(0f, 1.4f, 2.18f), new Vector3(1.7f, 0.6f, 0.08f), Look.GlassMat);
            Build.Box(root, "LightBar", new Vector3(0f, 1.95f, 0.2f), new Vector3(1.2f, 0.16f, 0.45f), Look.DarkMat);
            var red = Build.Box(root, "SirenR", new Vector3(-0.35f, 2.08f, 0.2f), new Vector3(0.4f, 0.14f, 0.36f), Look.AmbulanceRedMat);
            var blue = Build.Box(root, "SirenB", new Vector3(0.35f, 2.08f, 0.2f), new Vector3(0.4f, 0.14f, 0.36f), Look.Make(new Color(0.2f, 0.45f, 1f), new Color(0.3f, 0.5f, 1.6f)));
            Wheel(root, "WFL", new Vector3(-0.95f, 0.32f, 1.3f), 0.32f);
            Wheel(root, "WFR", new Vector3(0.95f, 0.32f, 1.3f), 0.32f);
            Wheel(root, "WRL", new Vector3(-0.95f, 0.32f, -1.3f), 0.32f);
            Wheel(root, "WRR", new Vector3(0.95f, 0.32f, -1.3f), 0.32f);

            var obstacle = FinishObstacle(root, ObstacleKind.Ambulance, new Vector3(2.1f, 2.0f, 4.6f));
            obstacle.SirenA = red.GetComponent<MeshRenderer>();
            obstacle.SirenB = blue.GetComponent<MeshRenderer>();
            return obstacle;
        }

        static ObstacleController MakeBike(Transform parent, int seed, bool rider)
        {
            var root = Build.Empty(rider ? "Bicycle" : "ParkedBike", parent);
            root.localScale = Vector3.one * 1.45f;
            var frame = Look.Car(seed + 3);
            Build.Cylinder(root, "FrontWheel", new Vector3(0f, 0.38f, 0.72f), new Vector3(0.58f, 0.045f, 0.58f), Look.BikeMat, new Vector3(0f, 0f, 90f));
            Build.Cylinder(root, "RearWheel", new Vector3(0f, 0.38f, -0.58f), new Vector3(0.58f, 0.045f, 0.58f), Look.BikeMat, new Vector3(0f, 0f, 90f));
            Build.Box(root, "Frame", new Vector3(0f, 0.62f, 0.05f), new Vector3(0.07f, 0.07f, 1.18f), frame, new Vector3(16f, 0f, 0f));
            Build.Box(root, "Down", new Vector3(0f, 0.55f, 0.28f), new Vector3(0.06f, 0.42f, 0.06f), frame, new Vector3(28f, 0f, 0f));
            Build.Box(root, "Stem", new Vector3(0f, 0.88f, 0.58f), new Vector3(0.06f, 0.42f, 0.06f), frame);
            Build.Box(root, "Bars", new Vector3(0f, 1.12f, 0.6f), new Vector3(0.78f, 0.06f, 0.06f), Look.ChromeMat);
            Build.Box(root, "Seat", new Vector3(0f, 0.95f, -0.28f), new Vector3(0.16f, 0.08f, 0.3f), Look.DarkMat);
            if (rider)
            {
                var clothes = Look.Car(seed);
                Build.Cylinder(root, "Rider", new Vector3(0f, 1.18f, -0.02f), new Vector3(0.34f, 0.4f, 0.34f), clothes);
                Build.Sphere(root, "Head", new Vector3(0f, 1.68f, 0.1f), 0.3f, Look.RiderMat);
            }

            return FinishObstacle(root, ObstacleKind.Bicycle, new Vector3(0.9f, 1.9f, 1.7f));
        }

        static ObstacleController FinishObstacle(Transform root, ObstacleKind kind, Vector3 colliderSize)
        {
            var box = root.gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, colliderSize.y * 0.45f, 0f);
            box.size = colliderSize;
            box.isTrigger = true;
            var obstacle = root.gameObject.AddComponent<ObstacleController>();
            obstacle.Kind = kind;
            obstacle.CollectWheels();
            return obstacle;
        }

        static void Wheel(Transform parent, string name, Vector3 pos, float radius = 0.38f)
        {
            var wheel = Build.Cylinder(parent, name, pos, new Vector3(radius, 0.12f, radius), Look.DarkMat, new Vector3(0f, 0f, 90f));
            wheel.gameObject.AddComponent<SpinWithSpeed>();
        }
    }

    public class SpinWithSpeed : MonoBehaviour
    {
        public static float Speed;

        void Update()
        {
            transform.Rotate(Vector3.forward, Speed * 90f * Time.deltaTime, Space.Self);
        }
    }
}
