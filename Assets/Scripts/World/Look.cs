using UnityEngine;

namespace OutOfWay
{
    /// <summary>Shared colors, generated textures, and URP materials.</summary>
    public static class Look
    {
        public static readonly Color Asphalt = new(0.16f, 0.16f, 0.18f);
        public static readonly Color Lane = new(0.95f, 0.82f, 0.18f);
        public static readonly Color Sidewalk = new(0.62f, 0.60f, 0.56f);
        public static readonly Color Grass = new(0.30f, 0.48f, 0.24f);
        public static readonly Color Curb = new(0.78f, 0.76f, 0.70f);
        public static readonly Color BusBody = new(0.93f, 0.74f, 0.16f);
        public static readonly Color BusStripe = new(0.10f, 0.38f, 0.42f);
        public static readonly Color BusDark = new(0.12f, 0.12f, 0.14f);
        public static readonly Color Glass = new(0.18f, 0.28f, 0.34f);
        public static readonly Color Chrome = new(0.72f, 0.74f, 0.76f);
        public static readonly Color Headlight = new(1f, 0.95f, 0.75f);
        public static readonly Color Taillight = new(0.85f, 0.12f, 0.10f);
        public static readonly Color Ambulance = new(0.93f, 0.93f, 0.94f);
        public static readonly Color AmbulanceRed = new(0.82f, 0.12f, 0.14f);
        public static readonly Color BikeFrame = new(0.15f, 0.15f, 0.18f);
        public static readonly Color Rider = new(0.22f, 0.34f, 0.55f);
        public static readonly Color Fog = new(0.62f, 0.74f, 0.82f);

        public static readonly Color[] BuildingWalls =
        {
            new(0.78f, 0.52f, 0.40f),
            new(0.86f, 0.78f, 0.64f),
            new(0.55f, 0.58f, 0.60f),
            new(0.70f, 0.42f, 0.36f),
            new(0.46f, 0.58f, 0.56f),
            new(0.90f, 0.86f, 0.78f),
            new(0.40f, 0.44f, 0.50f),
            new(0.72f, 0.62f, 0.48f)
        };

        public static readonly Color[] CarBodies =
        {
            new(0.78f, 0.18f, 0.16f),
            new(0.16f, 0.32f, 0.62f),
            new(0.92f, 0.92f, 0.90f),
            new(0.12f, 0.12f, 0.14f),
            new(0.20f, 0.55f, 0.38f),
            new(0.90f, 0.55f, 0.12f),
            new(0.55f, 0.22f, 0.55f)
        };

        public static Material Lit;
        public static Material BusMat;
        public static Material BusStripeMat;
        public static Material DarkMat;
        public static Material GlassMat;
        public static Material ChromeMat;
        public static Material HeadlightMat;
        public static Material TaillightMat;
        public static Material RoadMat;
        public static Material SidewalkMat;
        public static Material GrassMat;
        public static Material CurbMat;
        public static Material AmbulanceMat;
        public static Material AmbulanceRedMat;
        public static Material BikeMat;
        public static Material RiderMat;
        public static Material TrunkMat;
        public static Material LeafMat;
        public static Material[] CarMats;
        public static Material[] WallMats;
        public static Material[] FacadeMats;

        static Shader _lit;

        public static void Init()
        {
            if (Lit != null) return;

            _lit = Shader.Find("Universal Render Pipeline/Lit");
            if (_lit == null) _lit = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (_lit == null) _lit = Shader.Find("Standard");

            Lit = Make(Color.white, 0.2f, 0f);
            BusMat = Make(BusBody, 0.45f, 0.05f);
            BusStripeMat = Make(BusStripe, 0.4f, 0.05f);
            DarkMat = Make(BusDark, 0.25f, 0.1f);
            GlassMat = Make(Glass, 0.85f, 0.1f);
            ChromeMat = Make(Chrome, 0.7f, 0.6f);
            HeadlightMat = Make(Headlight, 0.9f, 0.1f, Headlight * 1.8f);
            TaillightMat = Make(Taillight, 0.6f, 0.1f, Taillight * 1.4f);
            AmbulanceMat = Make(Ambulance, 0.35f, 0.05f);
            AmbulanceRedMat = Make(AmbulanceRed, 0.4f, 0.05f, AmbulanceRed * 0.6f);
            BikeMat = Make(BikeFrame, 0.5f, 0.4f);
            RiderMat = Make(Rider, 0.25f, 0.05f);
            TrunkMat = Make(new Color(0.35f, 0.22f, 0.12f), 0.15f, 0f);
            LeafMat = Make(new Color(0.22f, 0.46f, 0.2f), 0.1f, 0f);

            RoadMat = Make(Asphalt, 0.15f, 0f);
            RoadMat.mainTexture = Textures.Road();
            RoadMat.mainTextureScale = new Vector2(1f, 6f);

            SidewalkMat = Make(Sidewalk, 0.12f, 0f);
            SidewalkMat.mainTexture = Textures.Noise(64, Sidewalk, 0.08f);
            GrassMat = Make(Grass, 0.08f, 0f);
            GrassMat.mainTexture = Textures.Noise(64, Grass, 0.12f);
            CurbMat = Make(Curb, 0.2f, 0f);

            CarMats = new Material[CarBodies.Length];
            for (int i = 0; i < CarBodies.Length; i++)
                CarMats[i] = Make(CarBodies[i], 0.4f, 0.08f);

            WallMats = new Material[BuildingWalls.Length];
            FacadeMats = new Material[BuildingWalls.Length];
            for (int i = 0; i < BuildingWalls.Length; i++)
            {
                WallMats[i] = Make(BuildingWalls[i], 0.12f, 0f);
                FacadeMats[i] = Make(BuildingWalls[i], 0.12f, 0f);
                FacadeMats[i].mainTexture = Textures.Facade(i * 97 + 3, BuildingWalls[i]);
            }
        }

        public static Material Make(Color color, float smoothness, float metallic, Color? emission = null)
        {
            var mat = new Material(_lit) { color = color };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (emission.HasValue)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emission.Value);
                }
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            return mat;
        }

        public static Material Car(int seed) => CarMats[Mathf.Abs(seed) % CarMats.Length];
        public static Material Wall(int seed) => WallMats[Mathf.Abs(seed) % WallMats.Length];
        public static Material Facade(int seed) => FacadeMats[Mathf.Abs(seed) % FacadeMats.Length];
    }

    public static class Textures
    {
        public static Texture2D Road()
        {
            const int w = 64;
            const int h = 256;
            var tex = new Texture2D(w, h) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            var asphalt = Look.Asphalt;
            var yellow = Look.Lane;
            var white = new Color(0.88f, 0.88f, 0.86f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float n = (Mathf.PerlinNoise(x * 0.18f, y * 0.11f) - 0.5f) * 0.07f;
                    var c = asphalt + new Color(n, n, n);
                    if (x <= 3 || x >= w - 4) c = Color.Lerp(c, white, 0.85f);
                    bool dash = (y % 40) < 22;
                    if (dash && x >= 30 && x <= 33) c = yellow;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D Facade(int seed, Color wall)
        {
            var rng = new System.Random(seed);
            const int cols = 8;
            const int rows = 12;
            const int cw = 8;
            const int ch = 10;
            var tex = new Texture2D(cols * cw, rows * ch)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var mortar = wall * 0.78f;
            var windowOff = new Color(0.08f, 0.10f, 0.14f);
            var windowOn = new Color(0.95f, 0.86f, 0.52f);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    bool edge = row == 0 || col == 0 || col == cols - 1;
                    bool lit = !edge && rng.NextDouble() > 0.18;
                    var fill = !lit ? wall : (rng.NextDouble() > 0.62 ? windowOn : windowOff);

                    for (int py = 0; py < ch; py++)
                    {
                        for (int px = 0; px < cw; px++)
                        {
                            bool border = px == 0 || py == 0;
                            tex.SetPixel(col * cw + px, row * ch + py, border ? mortar : fill);
                        }
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D Noise(int size, Color baseColor, float amount)
        {
            var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat };
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = (Mathf.PerlinNoise(x * 0.22f, y * 0.22f) - 0.5f) * amount;
                    tex.SetPixel(x, y, baseColor + new Color(n, n, n));
                }
            }

            tex.Apply();
            return tex;
        }
    }

    public static class Build
    {
        public static Transform Box(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, Vector3 euler = default)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.transform.localEulerAngles = euler;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.Destroy(go.GetComponent<Collider>());
            return go.transform;
        }

        public static Transform Cylinder(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, Vector3 euler = default)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.transform.localEulerAngles = euler;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.Destroy(go.GetComponent<Collider>());
            return go.transform;
        }

        public static Transform Sphere(Transform parent, string name, Vector3 pos, float scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = Vector3.one * scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.Destroy(go.GetComponent<Collider>());
            return go.transform;
        }

        public static Transform Empty(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            return go.transform;
        }
    }
}
