using UnityEngine;

namespace OutOfWay
{
    /// <summary>
    /// Designer Maya block: Street, Buildings, StreetAndBuildings in Resources/Art.
    /// Scaled so the road matches the bus, then tiled along Z. Buildings in the FBX
    /// sit on one sidewalk, so a mirrored copy fills the other side.
    /// </summary>
    public static class CityKit
    {
        public const string StreetAndBuildingsPath = "Art/StreetAndBuildings";
        public const string BuildingsPath = "Art/Buildings";
        public const string StreetPath = "Art/Street";
        public const float TargetRoadWidth = 8.2f;

        public static bool Ready { get; private set; }
        public static float TileLength { get; private set; } = 32f;
        public static float WalkX { get; private set; } = 5.1f;

        static GameObject _block;
        static GameObject _buildings;
        static GameObject _street;
        static float _scale = 1f;
        static bool _tried;

        public static void Bind(GameObject block, GameObject buildings, GameObject street)
        {
            _block = block;
            _buildings = buildings;
            _street = street;
            _tried = true;
            Ready = _block != null || _street != null;
            if (!Ready)
            {
                Debug.LogWarning("CityKit: assign StreetAndBuildings / Buildings / Street on the OutOfWay object. Falling back to cubes.");
                return;
            }

            MeasureScale(_block != null ? _block : _street);
        }

        public static void Load()
        {
            if (_tried) return;
            Bind(null, null, null);
        }

        public static void Place(Transform tile, int seed)
        {
            Load();
            if (!Ready) return;

            var kit = Spawn(_block != null ? _block : _street, tile, new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, TileLength * 0.5f), seed);
            PaintStreet(kit);

            if (_buildings == null) return;

            var origin = new Vector3(0f, 0f, TileLength * 0.5f);
            Spawn(_buildings, tile, new Vector3(-1f, 1f, 1f), origin, seed + 17);

            float shift = TileLength * 0.42f;
            if (shift > 10f)
            {
                var back = origin + new Vector3(0f, 0f, -shift);
                Spawn(_buildings, tile, new Vector3(1f, 1f, 1f), back, seed + 33);
                Spawn(_buildings, tile, new Vector3(-1f, 1f, 1f), back, seed + 51);
            }
        }

        static GameObject Spawn(GameObject prefab, Transform tile, Vector3 sign, Vector3 localPos, int seed)
        {
            var go = Object.Instantiate(prefab, tile);
            Strip(go);
            go.transform.localScale = new Vector3(sign.x * _scale, sign.y * _scale, sign.z * _scale);
            go.transform.localPosition = localPos;
            SitOnGround(go);
            PaintBuildings(go, seed);
            return go;
        }

        static void MeasureScale(GameObject prefab)
        {
            var temp = Object.Instantiate(prefab);
            temp.hideFlags = HideFlags.HideAndDontSave;
            var plane = FindNamed(temp.transform, "plane");
            var road = plane != null ? plane.GetComponent<Renderer>() : null;
            float roadW = road != null ? road.bounds.size.x : Combined(temp).size.x * 0.29f;
            if (roadW < 0.0001f) roadW = 1f;
            _scale = TargetRoadWidth / roadW;
            temp.transform.localScale = Vector3.one * _scale;
            TileLength = Mathf.Max(24f, Combined(temp).size.z);
            WalkX = TargetRoadWidth * 0.5f + 1.2f;
            Object.DestroyImmediate(temp);
        }

        static void SitOnGround(GameObject root)
        {
            var b = Combined(root);
            if (b.size.sqrMagnitude < 0.0001f) return;
            root.transform.position += new Vector3(0f, -b.min.y, 0f);
        }

        static void PaintStreet(GameObject root)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>())
            {
                string n = r.gameObject.name.ToLowerInvariant();
                if (n.Contains("plane"))
                    r.sharedMaterial = Look.RoadMat;
                else if (n.Contains("pcube1") || n.Contains("pcube2"))
                    r.sharedMaterial = Look.SidewalkMat;
            }
        }

        static void PaintBuildings(GameObject root, int seed)
        {
            int i = 0;
            foreach (var r in root.GetComponentsInChildren<Renderer>())
            {
                string n = r.gameObject.name.ToLowerInvariant();
                if (n.Contains("plane") || n.Contains("pcube1") || n.Contains("pcube2"))
                    continue;
                r.sharedMaterial = (i % 2 == 0) ? Look.Facade(seed + i) : Look.Wall(seed + i);
                i++;
            }
        }

        static void Strip(GameObject root)
        {
            foreach (var cam in root.GetComponentsInChildren<Camera>(true))
            {
                cam.enabled = false;
                if (cam.gameObject == root) Object.DestroyImmediate(cam);
                else Object.DestroyImmediate(cam.gameObject);
            }

            foreach (var light in root.GetComponentsInChildren<Light>(true))
            {
                if (light.gameObject == root) Object.DestroyImmediate(light);
                else Object.DestroyImmediate(light.gameObject);
            }

            foreach (var col in root.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(col);
        }

        static Transform FindNamed(Transform t, string part)
        {
            if (t.name.ToLowerInvariant().Contains(part) && t.GetComponent<Renderer>() != null)
                return t;
            foreach (Transform c in t)
            {
                var found = FindNamed(c, part);
                if (found != null) return found;
            }

            return null;
        }

        static Bounds Combined(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }
    }
}
