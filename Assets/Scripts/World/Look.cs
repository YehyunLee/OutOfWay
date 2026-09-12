using UnityEngine;

namespace OutOfWay
{
    /// <summary>Shared colors, generated textures, and URP materials.</summary>
    public static class Look
    {
        public static readonly Color Asphalt = new(0.18f, 0.18f, 0.20f);
        public static readonly Color Lane = new(0.98f, 0.82f, 0.12f);
        public static readonly Color Sidewalk = new(0.72f, 0.70f, 0.64f);
        public static readonly Color Grass = new(0.34f, 0.62f, 0.28f);
        public static readonly Color Curb = new(0.86f, 0.84f, 0.76f);
        public static readonly Color BusBody = new(0.96f, 0.76f, 0.12f);
        public static readonly Color BusStripe = new(0.10f, 0.38f, 0.42f);
        public static readonly Color BusDark = new(0.12f, 0.12f, 0.14f);
        public static readonly Color Glass = new(0.20f, 0.32f, 0.40f);
        public static readonly Color Chrome = new(0.78f, 0.80f, 0.82f);
        public static readonly Color Headlight = new(1f, 0.95f, 0.72f);
        public static readonly Color Taillight = new(0.90f, 0.12f, 0.10f);
        public static readonly Color Ambulance = new(0.95f, 0.95f, 0.96f);
        public static readonly Color AmbulanceRed = new(0.86f, 0.10f, 0.12f);
        public static readonly Color BikeFrame = new(0.12f, 0.12f, 0.14f);
        public static readonly Color Rider = new(0.18f, 0.40f, 0.72f);
        public static readonly Color Sky = new(0.55f, 0.80f, 0.98f);
        public static readonly Color Hedge = new(0.16f, 0.55f, 0.22f);

        public static readonly Color[] BuildingWalls =
        {
            new(0.94f, 0.58f, 0.36f),
            new(0.98f, 0.90f, 0.72f),
            new(0.88f, 0.38f, 0.28f),
            new(0.98f, 0.95f, 0.88f),
            new(0.38f, 0.58f, 0.82f),
            new(0.82f, 0.50f, 0.28f),
            new(0.58f, 0.74f, 0.42f),
            new(0.92f, 0.72f, 0.52f)
        };

        public static readonly Color[] ShopFronts =
        {
            new(0.88f, 0.14f, 0.16f),
            new(0.12f, 0.36f, 0.82f),
            new(0.96f, 0.52f, 0.08f),
            new(0.12f, 0.58f, 0.36f),
            new(0.58f, 0.18f, 0.58f),
            new(0.10f, 0.12f, 0.16f)
        };

        public static readonly Color[] CarBodies =
        {
            new(0.86f, 0.14f, 0.12f),
            new(0.12f, 0.32f, 0.72f),
            new(0.96f, 0.96f, 0.94f),
            new(0.10f, 0.10f, 0.12f),
            new(0.12f, 0.62f, 0.36f),
            new(0.96f, 0.55f, 0.08f),
            new(0.62f, 0.18f, 0.62f)
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
        public static Material HedgeMat;
        public static Material DashMat;
        public static Material WindowLitMat;
        public static Material WindowDarkMat;
        public static Material[] CarMats;
        public static Material[] WallMats;
        public static Material[] FacadeMats;
        public static Material[] ShopMats;

        static Shader _shader;

        public static void Init()
        {
            if (Lit != null) return;

            _shader = Shader.Find("Universal Render Pipeline/Unlit")
                      ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                      ?? Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Unlit/Color")
                      ?? Shader.Find("Sprites/Default");

            Lit = Make(Color.white);
            BusMat = Make(BusBody);
            BusStripeMat = Make(BusStripe);
            DarkMat = Make(BusDark);
            GlassMat = Make(Glass);
            ChromeMat = Make(Chrome);
            HeadlightMat = Make(Headlight, Headlight * 2f);
            TaillightMat = Make(Taillight, Taillight * 1.8f);
            AmbulanceMat = Make(Ambulance);
            AmbulanceRedMat = Make(AmbulanceRed, AmbulanceRed);
            BikeMat = Make(BikeFrame);
            RiderMat = Make(Rider);
            TrunkMat = Make(new Color(0.45f, 0.28f, 0.14f));
            LeafMat = Make(new Color(0.16f, 0.62f, 0.22f));
            HedgeMat = Make(Hedge);
            DashMat = Make(Lane);
            WindowLitMat = Make(new Color(1f, 0.92f, 0.52f), new Color(1.2f, 1f, 0.45f));
            WindowDarkMat = Make(new Color(0.10f, 0.16f, 0.28f));

            RoadMat = Make(Asphalt, null, Textures.Road(), new Vector2(1f, 8f));
            SidewalkMat = Make(Sidewalk, null, Textures.Noise(64, Sidewalk, 0.12f), Vector2.one * 4f);
            GrassMat = Make(Grass, null, Textures.Noise(64, Grass, 0.18f), Vector2.one * 6f);
            CurbMat = Make(Curb);

            CarMats = new Material[CarBodies.Length];
            for (int i = 0; i < CarBodies.Length; i++)
                CarMats[i] = Make(CarBodies[i]);

            WallMats = new Material[BuildingWalls.Length];
            FacadeMats = new Material[BuildingWalls.Length];
            for (int i = 0; i < BuildingWalls.Length; i++)
            {
                WallMats[i] = Make(BuildingWalls[i]);
                FacadeMats[i] = Make(BuildingWalls[i], null, Textures.Facade(i * 97 + 3, BuildingWalls[i]));
            }

            ShopMats = new Material[ShopFronts.Length];
            for (int i = 0; i < ShopFronts.Length; i++)
                ShopMats[i] = Make(ShopFronts[i]);
        }

        public static Material Make(Color color, Color? emission = null, Texture tex = null, Vector2? tiling = null)
        {
            var mat = new Material(_shader);
            Paint(mat, color, tex, tiling);
            if (!emission.HasValue) return mat;

            var glow = emission.Value;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", glow);
            }

            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            return mat;
        }

        public static void Paint(Material mat, Color color, Texture tex = null, Vector2? tiling = null)
        {
            mat.color = color;
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (tex == null) return;
            mat.mainTexture = tex;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            var scale = tiling ?? Vector2.one;
            mat.mainTextureScale = scale;
        }

        public static Material Car(int seed) => CarMats[Mathf.Abs(seed) % CarMats.Length];
        public static Material Wall(int seed) => WallMats[Mathf.Abs(seed) % WallMats.Length];
        public static Material Facade(int seed) => FacadeMats[Mathf.Abs(seed) % FacadeMats.Length];
        public static Material Shop(int seed) => ShopMats[Mathf.Abs(seed) % ShopMats.Length];
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
            var white = new Color(0.92f, 0.92f, 0.90f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float n = (Mathf.PerlinNoise(x * 0.18f, y * 0.11f) - 0.5f) * 0.08f;
                    var c = asphalt + new Color(n, n, n);
                    if (x <= 4 || x >= w - 5) c = Color.Lerp(c, white, 0.9f);
                    bool dash = (y % 36) < 20;
                    if (dash && x >= 29 && x <= 34) c = yellow;
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

            var mortar = wall * 0.72f;
            var windowOff = new Color(0.08f, 0.12f, 0.20f);
            var windowOn = new Color(1f, 0.90f, 0.48f);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    bool edge = row == 0 || col == 0 || col == cols - 1;
                    bool lit = !edge && rng.NextDouble() > 0.12;
                    var fill = !lit ? wall : (rng.NextDouble() > 0.45 ? windowOn : windowOff);

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
