using UnityEngine;

namespace OutOfWay
{
    public class EndlessCity : MonoBehaviour
    {
        public const float TileLength = 32f;
        const int TileCount = 12;
        const float RoadHalf = 4.0f;

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
            float recycleBehind = _bus.position.z - 18f;
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

            Build.Box(tile, "Grass", new Vector3(0f, -0.06f, TileLength * 0.5f), new Vector3(48f, 0.1f, TileLength), Look.GrassMat);
            Build.Box(tile, "Road", new Vector3(0f, 0.02f, TileLength * 0.5f), new Vector3(7.6f, 0.14f, TileLength), Look.RoadMat);
            Build.Box(tile, "WalkL", new Vector3(-5.4f, 0.08f, TileLength * 0.5f), new Vector3(3.0f, 0.14f, TileLength), Look.SidewalkMat);
            Build.Box(tile, "WalkR", new Vector3(5.4f, 0.08f, TileLength * 0.5f), new Vector3(3.0f, 0.14f, TileLength), Look.SidewalkMat);
            Build.Box(tile, "CurbL", new Vector3(-3.9f, 0.16f, TileLength * 0.5f), new Vector3(0.22f, 0.22f, TileLength), Look.CurbMat);
            Build.Box(tile, "CurbR", new Vector3(3.9f, 0.16f, TileLength * 0.5f), new Vector3(0.22f, 0.22f, TileLength), Look.CurbMat);
            Build.Box(tile, "HedgeL", new Vector3(-6.7f, 0.55f, TileLength * 0.5f), new Vector3(0.45f, 0.9f, TileLength - 1f), Look.HedgeMat);
            Build.Box(tile, "HedgeR", new Vector3(6.7f, 0.55f, TileLength * 0.5f), new Vector3(0.45f, 0.9f, TileLength - 1f), Look.HedgeMat);

            PlaceDashes(tile);
            PlaceLamps(tile, -4.2f, rng);
            PlaceLamps(tile, 4.2f, rng);
            FillSide(tile, -1f, rng, seed);
            FillSide(tile, 1f, rng, seed + 17);
            PlaceSidewalkBikes(tile, rng);
            PlaceParked(tile, rng);

            if (seed % 3 == 0)
                PlaceCrosswalk(tile);

            return tile;
        }

        static void PlaceDashes(Transform tile)
        {
            for (float z = 2f; z < TileLength - 1f; z += 4.2f)
                Build.Box(tile, "Dash", new Vector3(0f, 0.11f, z), new Vector3(0.18f, 0.04f, 2.1f), Look.DashMat);
        }

        static void PlaceCrosswalk(Transform tile)
        {
            for (int i = 0; i < 7; i++)
            {
                float x = -2.6f + i * 0.85f;
                Build.Box(tile, "Stripe", new Vector3(x, 0.11f, 4f), new Vector3(0.42f, 0.04f, 2.4f), Look.CurbMat);
            }
        }

        static void PlaceLamps(Transform tile, float x, System.Random rng)
        {
            int count = 3;
            for (int i = 0; i < count; i++)
            {
                float z = 3f + i * (TileLength / count);
                var lamp = Build.Empty("Lamp", tile);
                lamp.localPosition = new Vector3(x, 0f, z);
                Build.Cylinder(lamp, "Pole", new Vector3(0f, 2.4f, 0f), new Vector3(0.1f, 2.4f, 0.1f), Look.DarkMat);
                Build.Box(lamp, "Arm", new Vector3(Mathf.Sign(x) * -0.45f, 4.7f, 0f), new Vector3(0.9f, 0.1f, 0.1f), Look.DarkMat);
                Build.Sphere(lamp, "Bulb", new Vector3(Mathf.Sign(x) * -0.85f, 4.5f, 0f), 0.28f, Look.HeadlightMat);
            }
        }

        static void FillSide(Transform tile, float side, System.Random rng, int seed)
        {
            float x = side * 9.4f;
            float cursor = 0.6f;
            int buildingIndex = 0;
            while (cursor < TileLength - 1.2f)
            {
                float depth = 5.5f + (float)rng.NextDouble() * 2.4f;
                float width = 4.6f + (float)rng.NextDouble() * 3.2f;
                if (cursor + width > TileLength - 0.4f) width = TileLength - cursor - 0.4f;
                if (width < 3.2f) break;

                float height = 6.5f + (float)rng.NextDouble() * 10f;
                MakeBuilding(tile, new Vector3(x, 0f, cursor + width * 0.5f), width, height, depth, seed + buildingIndex * 13, side);
                buildingIndex++;
                cursor += width + 0.35f;
            }
        }

        static void MakeBuilding(Transform tile, Vector3 pos, float width, float height, float depth, int seed, float side)
        {
            var root = Build.Empty("Building", tile);
            root.localPosition = pos;
            var wall = Look.Wall(seed);
            var facade = Look.Facade(seed);
            var shop = Look.Shop(seed);

            Build.Box(root, "Mass", new Vector3(0f, height * 0.5f, 0f), new Vector3(depth, height, width), wall);
            float faceX = -side * (depth * 0.5f + 0.04f);
            Build.Box(root, "Facade", new Vector3(faceX, height * 0.55f, 0f), new Vector3(0.08f, height * 0.88f, width * 0.94f), facade);
            Build.Box(root, "Shop", new Vector3(faceX - side * 0.05f, 1.35f, 0f), new Vector3(0.12f, 2.6f, width * 0.9f), shop);
            Build.Box(root, "Awning", new Vector3(faceX - side * 0.7f, 2.7f, 0f), new Vector3(1.3f, 0.12f, width * 0.82f), shop);
            Build.Box(root, "Door", new Vector3(faceX - side * 0.12f, 1.05f, 0f), new Vector3(0.08f, 2.0f, 0.9f), Look.DarkMat);

            int floors = Mathf.Max(2, Mathf.FloorToInt((height - 3.2f) / 1.7f));
            int cols = Mathf.Max(2, Mathf.FloorToInt(width / 1.35f));
            for (int floor = 0; floor < floors; floor++)
            {
                float wy = 3.6f + floor * 1.7f;
                if (wy > height - 0.8f) break;
                for (int col = 0; col < cols; col++)
                {
                    float wz = -width * 0.35f + col * (width * 0.7f / Mathf.Max(1, cols - 1));
                    var glass = (seed + floor + col) % 3 == 0 ? Look.WindowLitMat : Look.WindowDarkMat;
                    Build.Box(root, "Win", new Vector3(faceX - side * 0.06f, wy, wz), new Vector3(0.1f, 0.9f, 0.7f), glass);
                }
            }

            if (height > 11f)
                Build.Box(root, "RoofBox", new Vector3(0f, height + 0.55f, 0f), new Vector3(1.4f, 1.0f, 1.8f), Look.DarkMat);

            if (seed % 2 == 0)
                PlaceTree(tile, new Vector3(side * 6.15f, 0f, pos.z), new System.Random(seed));
        }

        static void PlaceTree(Transform tile, Vector3 pos, System.Random rng)
        {
            var tree = Build.Empty("Tree", tile);
            tree.localPosition = pos;
            float trunk = 1.1f + (float)rng.NextDouble() * 0.4f;
            Build.Cylinder(tree, "Trunk", new Vector3(0f, trunk, 0f), new Vector3(0.22f, trunk, 0.22f), Look.TrunkMat);
            Build.Sphere(tree, "Canopy", new Vector3(0f, trunk * 2f + 0.55f, 0f), 1.8f + (float)rng.NextDouble() * 0.5f, Look.LeafMat);
        }

        static void PlaceParked(Transform tile, System.Random rng)
        {
            if (rng.NextDouble() > 0.55) return;
            float side = rng.NextDouble() < 0.5 ? -1f : 1f;
            var car = VehicleFactory.Park(VehicleFactory.MakeObstacle(ObstacleKind.Car, tile, rng.Next()));
            car.transform.localPosition = new Vector3(side * 5.45f, 0f, 8f + (float)rng.NextDouble() * 14f);
            car.transform.localEulerAngles = new Vector3(0f, side < 0 ? 180f : 0f, 0f);
        }

        static void PlaceSidewalkBikes(Transform tile, System.Random rng)
        {
            int count = 2 + rng.Next(3);
            for (int i = 0; i < count; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                bool riding = rng.NextDouble() < 0.35;
                var bike = VehicleFactory.MakeObstacle(ObstacleKind.Bicycle, tile, rng.Next());
                if (!riding)
                {
                    var rider = bike.transform.Find("Rider");
                    var head = bike.transform.Find("Head");
                    if (rider != null) Object.Destroy(rider.gameObject);
                    if (head != null) Object.Destroy(head.gameObject);
                    bike.transform.localEulerAngles = new Vector3(0f, side < 0 ? 185f : -5f, side * 12f);
                }
                else
                {
                    bike.transform.localEulerAngles = new Vector3(0f, side < 0 ? 180f : 0f, 0f);
                }

                VehicleFactory.Park(bike);
                float x = riding ? side * 4.55f : side * 4.85f;
                float z = 3f + i * 6.5f + (float)rng.NextDouble() * 2f;
                bike.transform.localPosition = new Vector3(x, 0f, z);
            }
        }

        public static bool OnRoad(float x) => Mathf.Abs(x) < RoadHalf;
    }
}
