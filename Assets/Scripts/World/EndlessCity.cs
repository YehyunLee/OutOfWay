using UnityEngine;

namespace OutOfWay
{
    public class EndlessCity : MonoBehaviour
    {
        public const float TileLength = 36f;
        const int TileCount = 10;
        const float RoadHalf = 4.2f;

        Transform _bus;
        Transform[] _tiles;
        float _nextZ;

        public void Generate(Transform bus)
        {
            _bus = bus;
            _tiles = new Transform[TileCount];
            _nextZ = -TileLength;
            for (int i = 0; i < TileCount; i++)
            {
                _tiles[i] = MakeTile(_nextZ, i + 11);
                _nextZ += TileLength;
            }
        }

        void Update()
        {
            if (_bus == null) return;
            float recycleBehind = _bus.position.z - 22f;
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i].position.z + TileLength < recycleBehind)
                {
                    Destroy(_tiles[i].gameObject);
                    _tiles[i] = MakeTile(_nextZ, Mathf.RoundToInt(_nextZ) + 41);
                    _nextZ += TileLength;
                }
            }
        }

        Transform MakeTile(float z, int seed)
        {
            var rng = new System.Random(seed);
            var tile = Build.Empty($"Tile_{seed}", transform);
            tile.position = new Vector3(0f, 0f, z);

            Build.Box(tile, "Grass", new Vector3(0f, -0.08f, TileLength * 0.5f), new Vector3(72f, 0.1f, TileLength), Look.GrassMat);
            Build.Box(tile, "Road", new Vector3(0f, 0.01f, TileLength * 0.5f), new Vector3(8.4f, 0.12f, TileLength), Look.RoadMat);
            Build.Box(tile, "WalkL", new Vector3(-6.1f, 0.06f, TileLength * 0.5f), new Vector3(3.6f, 0.12f, TileLength), Look.SidewalkMat);
            Build.Box(tile, "WalkR", new Vector3(6.1f, 0.06f, TileLength * 0.5f), new Vector3(3.6f, 0.12f, TileLength), Look.SidewalkMat);
            Build.Box(tile, "CurbL", new Vector3(-4.35f, 0.12f, TileLength * 0.5f), new Vector3(0.18f, 0.18f, TileLength), Look.CurbMat);
            Build.Box(tile, "CurbR", new Vector3(4.35f, 0.12f, TileLength * 0.5f), new Vector3(0.18f, 0.18f, TileLength), Look.CurbMat);

            PlaceLamps(tile, -4.7f, rng);
            PlaceLamps(tile, 4.7f, rng);

            FillSide(tile, -1f, rng, seed);
            FillSide(tile, 1f, rng, seed + 17);

            if (rng.NextDouble() < 0.35)
                PlaceParked(tile, rng);

            return tile;
        }

        static void PlaceLamps(Transform tile, float x, System.Random rng)
        {
            int count = 2 + rng.Next(2);
            for (int i = 0; i < count; i++)
            {
                float z = 4f + i * (TileLength / count) + (float)rng.NextDouble() * 2f;
                var lamp = Build.Empty("Lamp", tile);
                lamp.localPosition = new Vector3(x, 0f, z);
                Build.Cylinder(lamp, "Pole", new Vector3(0f, 2.2f, 0f), new Vector3(0.08f, 2.2f, 0.08f), Look.ChromeMat);
                Build.Box(lamp, "Arm", new Vector3(Mathf.Sign(x) * -0.35f, 4.25f, 0f), new Vector3(0.7f, 0.08f, 0.08f), Look.ChromeMat);
                Build.Sphere(lamp, "Bulb", new Vector3(Mathf.Sign(x) * -0.65f, 4.1f, 0f), 0.22f, Look.HeadlightMat);
            }
        }

        static void FillSide(Transform tile, float side, System.Random rng, int seed)
        {
            float x = side * 12.5f;
            float cursor = 1.5f;
            int buildingIndex = 0;
            while (cursor < TileLength - 2f)
            {
                float depth = 6f + (float)rng.NextDouble() * 5f;
                float width = 5.5f + (float)rng.NextDouble() * 5f;
                if (cursor + width > TileLength - 1f) break;

                if (rng.NextDouble() < 0.18)
                {
                    PlaceTree(tile, new Vector3(x + side * 1.2f, 0f, cursor + 1.5f), rng);
                    cursor += 4f;
                    continue;
                }

                float height = 7f + (float)rng.NextDouble() * 18f;
                MakeBuilding(tile, new Vector3(x, 0f, cursor + width * 0.5f), width, height, depth, seed + buildingIndex * 13, side);
                buildingIndex++;
                cursor += width + 0.8f + (float)rng.NextDouble() * 1.4f;
            }
        }

        static void MakeBuilding(Transform tile, Vector3 pos, float width, float height, float depth, int seed, float side)
        {
            var root = Build.Empty("Building", tile);
            root.localPosition = pos;
            var wall = Look.Wall(seed);
            var facade = Look.Facade(seed);

            Build.Box(root, "Mass", new Vector3(0f, height * 0.5f, 0f), new Vector3(depth, height, width), wall);
            float faceX = -side * (depth * 0.5f + 0.03f);
            var face = Build.Box(root, "Facade", new Vector3(faceX, height * 0.5f, 0f), new Vector3(0.06f, height * 0.96f, width * 0.92f), facade);
            face.localEulerAngles = new Vector3(0f, 0f, 0f);

            if (height > 12f && seed % 3 == 0)
                Build.Box(root, "RoofBox", new Vector3(0f, height + 0.7f, 0f), new Vector3(1.6f, 1.2f, 2.2f), Look.DarkMat);

            if (seed % 4 == 0)
                Build.Box(root, "Awning", new Vector3(faceX - side * 0.6f, 2.4f, 0f), new Vector3(1.2f, 0.1f, width * 0.7f), Look.BusStripeMat);
        }

        static void PlaceTree(Transform tile, Vector3 pos, System.Random rng)
        {
            var tree = Build.Empty("Tree", tile);
            tree.localPosition = pos;
            float trunk = 1.4f + (float)rng.NextDouble() * 0.6f;
            Build.Cylinder(tree, "Trunk", new Vector3(0f, trunk, 0f), new Vector3(0.18f, trunk, 0.18f), Look.TrunkMat);
            Build.Sphere(tree, "Canopy", new Vector3(0f, trunk * 2f + 0.4f, 0f), 1.6f + (float)rng.NextDouble() * 0.6f, Look.LeafMat);
        }

        static void PlaceParked(Transform tile, System.Random rng)
        {
            float side = rng.NextDouble() < 0.5 ? -1f : 1f;
            var car = VehicleFactory.MakeObstacle(ObstacleKind.Car, tile, rng.Next());
            car.transform.localPosition = new Vector3(side * 5.6f, 0f, 8f + (float)rng.NextDouble() * 18f);
            car.transform.localEulerAngles = new Vector3(0f, side < 0 ? 180f : 0f, 0f);
            car.Parked = true;
            car.enabled = false;
            var col = car.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            foreach (var spin in car.GetComponentsInChildren<SpinWithSpeed>())
                spin.enabled = false;
        }

        public static bool OnRoad(float x) => Mathf.Abs(x) < RoadHalf;
    }
}
