using UnityEngine;

namespace OutOfWay
{
    /// <summary>
    /// Places the designer FBX as the looping street tile.
    /// StreetAndBuildings / FullScene is the complete block.
    /// Also supports modular Street + Buildings + StreetLamp.
    /// </summary>
    public static class CityKit
    {
        public const float TargetRoadWidth = 8.2f;

        public static bool Ready { get; private set; }
        public static float TileLength { get; private set; } = 32f;

        static GameObject _block;
        static GameObject _buildings;
        static GameObject _street;
        static GameObject _lamp;
        static GameObject _tree;
        static GameObject _female;
        static GameObject _male;
        static float _scale = 1f;
        static bool _tried;

        public static void Bind(GameObject block, GameObject buildings, GameObject street, GameObject lamp = null, GameObject tree = null, GameObject female = null, GameObject male = null)
        {
            _block = block;
            _buildings = buildings;
            _street = street;
            _lamp = lamp;
            _tree = tree;
            _female = female;
            _male = male;
            _tried = true;
            Ready = _block != null || _street != null;
            if (!Ready)
            {
                Debug.LogWarning("CityKit: assign StreetAndBuildings or Street on OutOfWay.");
                return;
            }

            MeasureScale(_block != null ? _block : _street);

#if UNITY_EDITOR
            if (_tree == null)
                _tree = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/SM_Tree_01.fbx");
            if (_female == null)
                _female = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/SM_HumanFemale/SM_HumanFemale.fbx");
            if (_male == null)
                _male = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/SM_HumanMale/SM_HumanMale.fbx");
#endif

            if (_tree == null)
                _tree = Resources.Load<GameObject>("Models/SM_Tree_01");
            if (_female == null)
                _female = Resources.Load<GameObject>("Models/SM_HumanFemale");
            if (_male == null)
                _male = Resources.Load<GameObject>("Models/SM_HumanMale");
        }

        public static void Load()
        {
            if (_tried) return;
            Bind(null, null, null, null, null, null, null);
        }

        public static void Place(Transform tile)
        {
            Load();
            if (!Ready) return;

            var basePrefab = _block != null ? _block : _street;
            var block = Spawn(basePrefab, tile, Vector3.one);
            AlignRoad(block, tile);

            // If using separate modular street + buildings
            if (_block == null && _buildings != null)
            {
                var bRow = Spawn(_buildings, tile, Vector3.one);
                AlignRoad(bRow, tile);
            }

            // If lamp is provided separately and not already in the block
            if (_lamp != null && _block == null)
            {
                var lamp = Spawn(_lamp, tile, Vector3.one);
                AlignRoad(lamp, tile);
            }

            // Place street trees along sidewalks
            if (_tree != null)
            {
                PlantTrees(tile);
            }

            // Place pedestrians along sidewalks
            if (_female != null || _male != null)
            {
                PlacePedestrians(tile);
            }
        }

        static void PlantTrees(Transform tile)
        {
            if (_tree == null) return;
            float len = TileLength;

            // Staggered placement along left and right sidewalks
            PlantTree(tile, new Vector3(-5.4f, 0f, len * 0.22f), 45f);
            PlantTree(tile, new Vector3(5.4f, 0f, len * 0.50f), 135f);
            PlantTree(tile, new Vector3(-5.4f, 0f, len * 0.76f), 220f);
        }

        static void PlantTree(Transform tile, Vector3 offsetOnTile, float yaw)
        {
            var tree = Object.Instantiate(_tree, tile);
            Strip(tree);

            tree.transform.localPosition = Vector3.zero;
            tree.transform.localRotation = Quaternion.identity;
            tree.transform.localScale = Vector3.one;

            var b = Combined(tree);
            float currentH = Mathf.Max(0.1f, b.size.y);
            float targetHeight = 5.8f;
            float scale = targetHeight / currentH;
            tree.transform.localScale = Vector3.one * scale;

            b = Combined(tree);
            tree.transform.position = new Vector3(
                offsetOnTile.x - b.center.x,
                -b.min.y,
                tile.position.z + offsetOnTile.z - b.center.z
            );
            tree.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            Look.UseImported(tree);
        }

        static void PlacePedestrians(Transform tile)
        {
            float len = TileLength;

            if (_female != null)
            {
                PlacePedestrian(tile, _female, new Vector3(-5.2f, 0f, len * 0.36f), 90f);
            }

            if (_male != null)
            {
                PlacePedestrian(tile, _male, new Vector3(5.2f, 0f, len * 0.65f), -90f);
            }
        }

        static void PlacePedestrian(Transform tile, GameObject prefab, Vector3 offsetOnTile, float yaw)
        {
            if (prefab == null) return;
            var ped = Object.Instantiate(prefab, tile);
            Strip(ped);

            ped.transform.localPosition = Vector3.zero;
            ped.transform.localRotation = Quaternion.identity;
            ped.transform.localScale = Vector3.one;

            var b = Combined(ped);
            float currentH = Mathf.Max(0.1f, b.size.y);
            float targetHeight = 1.75f;
            float scale = targetHeight / currentH;
            ped.transform.localScale = Vector3.one * scale;

            b = Combined(ped);
            ped.transform.position = new Vector3(
                offsetOnTile.x - b.center.x,
                -b.min.y,
                tile.position.z + offsetOnTile.z - b.center.z
            );
            ped.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            Look.UseImported(ped);
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
            go.transform.position += new Vector3(-road.center.x, -road.min.y, tile.position.z - road.min.z);
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
            var road = FindRoad(go.transform);
            var renderer = road != null ? road.GetComponent<Renderer>() : null;
            return renderer != null ? renderer.bounds : Combined(go);
        }

        static Transform FindRoad(Transform root)
        {
            // Prefer renderer named "street" or "road" (excluding "lamp", "side", "building")
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var n = r.name.ToLowerInvariant();
                if ((n == "street" || n.Contains("road") || n == "plane") &&
                    !n.Contains("lamp") && !n.Contains("side") && !n.Contains("building"))
                    return r.transform;
            }
            // Fallback: any renderer containing "street" that is not a lamp
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var n = r.name.ToLowerInvariant();
                if (n.Contains("street") && !n.Contains("lamp"))
                    return r.transform;
            }
            return null;
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
