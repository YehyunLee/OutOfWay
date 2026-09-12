using UnityEngine;

namespace OutOfWay
{
    /// <summary>Loops the designer FBX street. No generated cubes, lamps, or props.</summary>
    public class EndlessCity : MonoBehaviour
    {
        const int TileCount = 8;
        const float RoadHalf = 4.0f;

        Transform _bus;
        Transform[] _tiles;
        float _nextZ;
        float _tileLength = 32f;

        public void Generate(Transform bus)
        {
            _bus = bus;
            CityKit.Load();
            if (!CityKit.Ready)
            {
                Debug.LogError("EndlessCity: assign StreetAndBuildings on OutOfWay. Landscape is assets only.");
                return;
            }

            _tileLength = CityKit.TileLength;
            _tiles = new Transform[TileCount];
            _nextZ = -_tileLength;
            for (int i = 0; i < TileCount; i++)
            {
                _tiles[i] = MakeTile(_nextZ);
                _nextZ += _tileLength;
            }
        }

        void Update()
        {
            if (_bus == null || _tiles == null) return;
            float recycleBehind = _bus.position.z - 22f;
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i].position.z + _tileLength < recycleBehind)
                {
                    Destroy(_tiles[i].gameObject);
                    _tiles[i] = MakeTile(_nextZ);
                    _nextZ += _tileLength;
                }
            }
        }

        Transform MakeTile(float z)
        {
            var tile = Build.Empty("Tile", transform);
            tile.position = new Vector3(0f, 0f, z);
            CityKit.Place(tile);
            return tile;
        }

        public static bool OnRoad(float x) => Mathf.Abs(x) < RoadHalf;
    }
}
