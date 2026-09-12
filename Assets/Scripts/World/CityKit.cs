using UnityEngine;

namespace OutOfWay
{
    /// <summary>
    /// Places the designer FBX as the looping street tile.
    /// StreetAndBuildings is the block. Buildings is mirrored onto the empty sidewalk.
    /// </summary>
    public static class CityKit
    {
        public const float TargetRoadWidth = 8.2f;

        public static bool Ready { get; private set; }
        public static float TileLength { get; private set; } = 32f;

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
                Debug.LogWarning("CityKit: assign StreetAndBuildings (and Buildings) on OutOfWay.");
                return;
            }

            MeasureScale(_block != null ? _block : _street);
        }

        public static void Load()
        {
            if (_tried) return;
            Bind(null, null, null);
        }

        public static void Place(Transform tile)
        {
            Load();
            if (!Ready) return;

            var block = Spawn(_block != null ? _block : _street, tile, Vector3.one);
            AlignRoad(block, tile);

            if (_buildings == null) return;
            var row = Spawn(_buildings, tile, new Vector3(-1f, 1f, 1f));
            MatchStreet(row, block);
        }

        static GameObject Spawn(GameObject prefab, Transform tile, Vector3 sign)
        {
            var go = Object.Instantiate(prefab, tile);
            Strip(go);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = new Vector3(sign.x * _scale, sign.y * _scale, sign.z * _scale);
            Look.UseImported(go);
            return go;
        }

        static void AlignRoad(GameObject go, Transform tile)
        {
            var road = RoadBounds(go);
            var all = Combined(go);
            go.transform.position += new Vector3(-road.center.x, -all.min.y, tile.position.z - road.min.z);
        }

        static void MatchStreet(GameObject row, GameObject block)
        {
            var buildings = Combined(row);
            var street = Combined(block);
            row.transform.position += new Vector3(0f, -buildings.min.y, street.min.z - buildings.min.z);
        }

        static void MeasureScale(GameObject prefab)
        {
            var temp = Object.Instantiate(prefab);
            temp.hideFlags = HideFlags.HideAndDontSave;
            var road = RoadBounds(temp);
            float roadW = road.size.x;
            if (roadW < 0.0001f) roadW = 1f;
            _scale = TargetRoadWidth / roadW;
            temp.transform.localScale = Vector3.one * _scale;
            TileLength = Mathf.Max(8f, RoadBounds(temp).size.z);
            Object.DestroyImmediate(temp);
        }

        static Bounds RoadBounds(GameObject go)
        {
            var plane = FindNamed(go.transform, "plane");
            var renderer = plane != null ? plane.GetComponent<Renderer>() : null;
            return renderer != null ? renderer.bounds : Combined(go);
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
