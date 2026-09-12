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

        [Header("Scene & Character Textures")]
        public static Texture2D TexStreet;
        public static Texture2D TexSides;
        public static Texture2D TexBuildingsSide2;
        public static Texture2D TexBuildingsSide3;
        public static Texture2D TexLamp;

        public static Texture2D TexFemaleDress;
        public static Texture2D TexFemaleSkin;
        public static Texture2D TexFemaleHair;
        public static Texture2D TexFemaleEyes;

        public static Texture2D TexMaleShirt;
        public static Texture2D TexMaleSkin;
        public static Texture2D TexMaleEyes;

        static Shader _shader;
        static readonly Dictionary<string, Material> ImportedByTag = new();
        static bool _texturesLoaded;

        public static void Init()
        {
            if (Lit != null) return;

            LoadTextures();

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

        public static void LoadTextures()
        {
            if (_texturesLoaded) return;
            _texturesLoaded = true;

            if (TexStreet == null) TexStreet = Resources.Load<Texture2D>("Textures/Street_BaseColor");
            if (TexSides == null) TexSides = Resources.Load<Texture2D>("Textures/Sides_BaseColor");
            if (TexBuildingsSide2 == null) TexBuildingsSide2 = Resources.Load<Texture2D>("Textures/Buildings_side_2_BaseColor");
            if (TexBuildingsSide3 == null) TexBuildingsSide3 = Resources.Load<Texture2D>("Textures/Buildings_side_3_BaseColor");
            if (TexLamp == null) TexLamp = Resources.Load<Texture2D>("Textures/Lamp_BaseColor");

            if (TexFemaleDress == null) TexFemaleDress = Resources.Load<Texture2D>("Textures/Female_Dress");
            if (TexFemaleSkin == null) TexFemaleSkin = Resources.Load<Texture2D>("Textures/Female_Skin");
            if (TexFemaleHair == null) TexFemaleHair = Resources.Load<Texture2D>("Textures/Female_Hair");
            if (TexFemaleEyes == null) TexFemaleEyes = Resources.Load<Texture2D>("Textures/Female_Eyes");

            if (TexMaleShirt == null) TexMaleShirt = Resources.Load<Texture2D>("Textures/Male_Shirt");
            if (TexMaleSkin == null) TexMaleSkin = Resources.Load<Texture2D>("Textures/Male_Skin");
            if (TexMaleEyes == null) TexMaleEyes = Resources.Load<Texture2D>("Textures/Male_Eyes");

#if UNITY_EDITOR
            if (TexStreet == null) TexStreet = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Textures/Street/full scene take 2_Street_BaseColor_ACEScg.png");
            if (TexSides == null) TexSides = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Textures/Sides/full scene take 2_Sides_BaseColor_ACEScg.png");
            if (TexBuildingsSide2 == null) TexBuildingsSide2 = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Textures/Buildings_side_2/full scene take 2_Buildings side 2_BaseColor_ACEScg.png");
            if (TexBuildingsSide3 == null) TexBuildingsSide3 = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Textures/Buildings_side_3/full scene take 2_Buildings side 3_BaseColor_ACEScg.png");
            if (TexLamp == null) TexLamp = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Textures/Lamp/full scene take 2_Lamp_BaseColor_ACEScg.png");

            if (TexFemaleDress == null) TexFemaleDress = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Characters/SM_HumanFemale/SM_HumanFemale_M_Dress_01_BaseColor_ACEScg.png");
            if (TexFemaleSkin == null) TexFemaleSkin = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Characters/SM_HumanFemale/SM_HumanFemale_M_Skin_01_BaseColor_ACEScg.png");
            if (TexFemaleHair == null) TexFemaleHair = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Characters/SM_HumanFemale/SM_HumanFemale_M_Hair_01_BaseColor_ACEScg.png");
            if (TexFemaleEyes == null) TexFemaleEyes = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Characters/SM_HumanFemale/SM_HumanFemale_M_Eyes_01_BaseColor_ACEScg.png");

            if (TexMaleShirt == null) TexMaleShirt = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Characters/SM_HumanMale/SM_HumanMale_M_Shirt_01_BaseColor_ACEScg.png");
            if (TexMaleSkin == null) TexMaleSkin = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Characters/SM_HumanMale/SM_HumanMale_M_Skin_01_BaseColor_ACEScg.png");
            if (TexMaleEyes == null) TexMaleEyes = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Characters/SM_HumanMale/SM_HumanMale_M_Eyes_01_BaseColor_ACEScg.png");
#endif
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
            if (Lit == null) Init();

            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                if (mats == null || mats.Length == 0)
                {
                    // FBX exported without materials — resolve directly from object name
                    r.sharedMaterial = FromImported(null, r.name);
                    continue;
                }

                for (int i = 0; i < mats.Length; i++)
                    mats[i] = FromImported(mats[i], r.name);
                r.sharedMaterials = mats;
            }
        }

        /// <summary>
        /// Maps a Maya material or object name onto the game palette.
        /// Returns <paramref name="fallback"/> when nothing matches.
        /// </summary>
        static Color PaletteFor(string rawName, Color fallback)
        {
            var n = (rawName ?? string.Empty).ToLowerInvariant();
            if (n.Contains("lambert5") || n.Contains("street") || n.Contains("road"))
                return new Color(0.24f, 0.25f, 0.27f);
            if (n.Contains("lambert6") || n.Contains("side") || n.Contains("walk") || n.Contains("curb"))
                return new Color(0.82f, 0.81f, 0.78f);
            if (n.Contains("lambert3") || n.Contains("building"))
                return new Color(0.86f, 0.48f, 0.35f);
            if (n.Contains("lambert4"))
                return new Color(0.35f, 0.58f, 0.65f);
            if (n.Contains("lambert7") || n.Contains("lamp") || n.Contains("pole"))
                return new Color(0.18f, 0.18f, 0.20f);
            if (n.Contains("bus"))
                return BusBody;
            if (n.Contains("wheelclinder") || n.Contains("clider") || n.Contains("wheelc"))
                return new Color(0.18f, 0.18f, 0.20f);
            if (n.Contains("wheel"))
                return BusDark;
            if (n.Contains("leaves") || n.Contains("leaf") || n.Contains("foliage"))
                return new Color(0.24f, 0.58f, 0.22f);
            if (n.Contains("branche") || n.Contains("trunk") || n.Contains("bark") || n.Contains("wood"))
                return new Color(0.36f, 0.24f, 0.16f);
            return fallback;
        }

        public static Material FromImported(Material source, string objectName = null)
        {
            if (Lit == null) Init();

            string matName = (source != null ? source.name : string.Empty).ToLowerInvariant();
            string objName = (objectName ?? string.Empty).ToLowerInvariant();
            string combined = $"{matName} {objName}".Trim();

            string cacheKey = $"{matName}___{objName}";
            if (ImportedByTag.TryGetValue(cacheKey, out var cached) && cached != null)
                return cached;

            Color color = Color.white;
            Texture tex = source != null && source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") : (source != null ? source.mainTexture : null);

            // If no embedded texture, match against scene, character, and foliage textures
            if (tex == null)
            {
                if (combined.Contains("street") || combined.Contains("road") || combined.Contains("lambert5"))
                {
                    tex = TexStreet;
                    color = tex != null ? Color.white : new Color(0.24f, 0.25f, 0.27f);
                }
                else if (combined.Contains("side") || combined.Contains("walk") || combined.Contains("curb") || combined.Contains("lambert6"))
                {
                    tex = TexSides;
                    color = tex != null ? Color.white : new Color(0.82f, 0.81f, 0.78f);
                }
                else if (combined.Contains("buildings_side_3") || combined.Contains("building 3") || combined.Contains("lambert4"))
                {
                    tex = TexBuildingsSide3;
                    color = tex != null ? Color.white : new Color(0.35f, 0.58f, 0.65f);
                }
                else if (combined.Contains("building") || combined.Contains("lambert3"))
                {
                    tex = TexBuildingsSide2;
                    color = tex != null ? Color.white : new Color(0.86f, 0.48f, 0.35f);
                }
                else if (combined.Contains("lamp") || combined.Contains("pole") || combined.Contains("lambert7"))
                {
                    tex = TexLamp;
                    color = tex != null ? Color.white : new Color(0.18f, 0.18f, 0.20f);
                }
                else if (combined.Contains("leaves") || combined.Contains("leaf") || combined.Contains("foliage"))
                {
                    color = new Color(0.24f, 0.58f, 0.22f); // lush green tree foliage
                }
                else if (combined.Contains("branche") || combined.Contains("trunk") || combined.Contains("bark") || combined.Contains("wood"))
                {
                    color = new Color(0.36f, 0.24f, 0.16f); // natural tree bark brown
                }
                else if (combined.Contains("dress") || combined.Contains("shirt") || combined.Contains("cloth"))
                {
                    if (combined.Contains("shirt")) tex = TexMaleShirt ?? TexFemaleDress;
                    else tex = TexFemaleDress ?? TexMaleShirt;
                    color = tex != null ? Color.white : new Color(0.85f, 0.32f, 0.28f);
                }
                else if (combined.Contains("skin"))
                {
                    tex = TexFemaleSkin ?? TexMaleSkin;
                    color = tex != null ? Color.white : new Color(0.88f, 0.72f, 0.60f);
                }
                else if (combined.Contains("hair"))
                {
                    tex = TexFemaleHair;
                    color = tex != null ? Color.white : new Color(0.22f, 0.16f, 0.12f);
                }
                else if (combined.Contains("eye"))
                {
                    tex = TexFemaleEyes ?? TexMaleEyes;
                    color = tex != null ? Color.white : new Color(0.15f, 0.15f, 0.18f);
                }
                else
                {
                    color = PaletteFor(combined, Color.gray);
                }
            }

            var mat = Make(color, null, tex);
            mat.name = (source != null ? source.name : objectName) + " (URP)";
            ImportedByTag[cacheKey] = mat;
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
