using System.Collections.Generic;
using UnityEngine;

namespace OutOfWay
{
    /// <summary>URP unlit materials for vehicles and imported meshes.</summary>
    public static class Look
    {
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
        public static Material AmbulanceMat;
        public static Material AmbulanceRedMat;
        public static Material BikeMat;
        public static Material RiderMat;
        public static Material[] CarMats;

        static Shader _shader;
        static readonly Dictionary<EntityId, Material> Imported = new();

        public static void Init()
        {
            if (Lit != null) return;

            _shader = Shader.Find("Universal Render Pipeline/Unlit")
                      ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                      ?? Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Unlit/Color")
                      ?? Shader.Find("Sprites/Default");
            if (_shader == null)
            {
                Debug.LogError("Look: no URP/Unlit shader found. Check the URP package.");
                return;
            }

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

            CarMats = new Material[CarBodies.Length];
            for (int i = 0; i < CarBodies.Length; i++)
                CarMats[i] = Make(CarBodies[i]);
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
            mat.mainTextureScale = tiling ?? Vector2.one;
        }

        public static Material Car(int seed) => CarMats[Mathf.Abs(seed) % CarMats.Length];

        public static void UseImported(GameObject root)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                if (mats == null || mats.Length == 0) continue;
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = FromImported(mats[i]);
                r.sharedMaterials = mats;
            }
        }

        public static Material FromImported(Material source)
        {
            if (Lit == null) Init();
            if (source == null) return Lit ?? Make(Color.gray);
            var id = source.GetEntityId();
            if (Imported.TryGetValue(id, out var cached) && cached != null)
                return cached;

            Color color = Color.gray;
            if (source.HasProperty("_BaseColor")) color = source.GetColor("_BaseColor");
            else if (source.HasProperty("_Color")) color = source.GetColor("_Color");
            else color = source.color;

            Texture tex = source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") : source.mainTexture;

            // If untextured default Maya gray, apply palette mapped from material name
            if (tex == null && Mathf.Abs(color.r - 0.5f) < 0.05f && Mathf.Abs(color.g - 0.5f) < 0.05f && Mathf.Abs(color.b - 0.5f) < 0.05f)
            {
                var n = source.name.ToLowerInvariant();
                if (n.Contains("lambert5") || n.Contains("street") || n.Contains("road"))
                    color = new Color(0.24f, 0.25f, 0.27f);
                else if (n.Contains("lambert6") || n.Contains("side") || n.Contains("walk") || n.Contains("curb"))
                    color = new Color(0.82f, 0.81f, 0.78f);
                else if (n.Contains("lambert3") || n.Contains("building"))
                    color = new Color(0.86f, 0.48f, 0.35f);
                else if (n.Contains("lambert4"))
                    color = new Color(0.35f, 0.58f, 0.65f);
                else if (n.Contains("lambert7") || n.Contains("lamp") || n.Contains("pole"))
                    color = new Color(0.18f, 0.18f, 0.20f);
            }

            var mat = Make(color, null, tex);
            mat.name = source.name + " (URP)";
            Imported[id] = mat;
            return mat;
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
