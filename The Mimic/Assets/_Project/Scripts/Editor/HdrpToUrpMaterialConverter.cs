using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TheMimic
{
    // Converts HDRP materials (broken/pink in this URP project) to URP/Lit in place:
    // menu The Mimic > Convert HDRP Materials To URP, run on the folder selected in the Project window.
    // Reads each material's serialized properties directly, so it works even though the
    // HDRP shaders are missing (Hidden/InternalErrorShader). Re-running skips converted materials.
    public static class HdrpToUrpMaterialConverter
    {
        const string DefaultFolder = "Assets/GhostbuGaming";

        static readonly string[] BaseMapNames = { "_BaseColorMap", "_BaseMap", "_MainTex", "_AlbedoMap", "_Albedo", "_DiffuseMap", "_Diffuse" };
        static readonly string[] NormalMapNames = { "_NormalMap", "_BumpMap", "_NormalMapOS" };
        static readonly string[] EmissiveMapNames = { "_EmissiveColorMap", "_EmissionMap" };

        [MenuItem("The Mimic/Convert HDRP Materials To URP")]
        public static void Convert()
        {
            string folder = SelectedFolder() ?? DefaultFolder;
            if (!AssetDatabase.IsValidFolder(folder))
            {
                EditorUtility.DisplayDialog("Convert HDRP Materials",
                    $"Folder not found: {folder}\n\nSelect the asset pack's folder in the Project window and run this again.", "OK");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folder });
            if (!EditorUtility.DisplayDialog("Convert HDRP Materials",
                $"Convert HDRP/broken materials under:\n{folder}\n({guids.Length} materials found)\n\n" +
                "This edits the materials IN PLACE. To restore originals later, re-import the pack's .unitypackage.",
                "Convert", "Cancel"))
                return;

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("[HdrpToUrp] URP Lit shader not found — is this a URP project?");
                return;
            }

            var report = new StringBuilder($"[HdrpToUrp] Conversion report for {folder}:\n");
            int converted = 0, skipped = 0, noBaseMap = 0;

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (mat == null)
                        continue;

                    EditorUtility.DisplayProgressBar("Converting HDRP materials", mat.name, (float)i / guids.Length);

                    if (!NeedsConversion(mat))
                    {
                        skipped++;
                        continue;
                    }

                    if (ConvertMaterial(mat, urpLit, report))
                        converted++;
                    else
                        noBaseMap++;
                    EditorUtility.SetDirty(mat);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            report.Insert(0, $"[HdrpToUrp] DONE: {converted} converted, {skipped} already fine/non-HDRP (skipped), {noBaseMap} converted without a base map (gray).\n");
            Debug.Log(report.ToString());
        }

        static string SelectedFolder()
        {
            foreach (Object obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path))
                    return path;
            }
            return null;
        }

        static bool NeedsConversion(Material mat)
        {
            if (mat.shader == null)
                return true;
            string name = mat.shader.name;
            return name == "Hidden/InternalErrorShader" || name.StartsWith("HDRP/") || name.StartsWith("Shader Graphs/");
        }

        static bool ConvertMaterial(Material mat, Shader urpLit, StringBuilder report)
        {
            SavedProperties saved = ReadSavedProperties(mat);

            mat.shader = urpLit;

            // Base color + map (fall back to any plausible texture so nothing stays pink)
            Texture baseMap = FirstTexture(saved, BaseMapNames) ?? GuessBaseMap(saved);
            bool hasBase = baseMap != null;
            if (hasBase)
            {
                mat.SetTexture("_BaseMap", baseMap);
                if (saved.TexScaleOffset.TryGetValue(baseMap, out var so))
                {
                    mat.SetTextureScale("_BaseMap", so.Item1);
                    mat.SetTextureOffset("_BaseMap", so.Item2);
                }
            }
            Color baseColor = saved.Colors.TryGetValue("_BaseColor", out var c) ? c
                : saved.Colors.TryGetValue("_Color", out c) ? c : Color.white;
            mat.SetColor("_BaseColor", baseColor);

            // Normal map
            Texture normal = FirstTexture(saved, NormalMapNames);
            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", saved.Floats.TryGetValue("_NormalScale", out float ns) ? ns : 1f);
                mat.EnableKeyword("_NORMALMAP");
            }

            // HDRP mask map (R=metallic, G=AO, A=smoothness) -> unpacked URP textures
            Texture mask = FirstTexture(saved, new[] { "_MaskMap" });
            bool usedMask = false;
            if (mask is Texture2D mask2D)
                usedMask = ApplyMaskMap(mat, mask2D, report);

            mat.SetFloat("_Metallic", saved.Floats.TryGetValue("_Metallic", out float m) ? m : 0f);
            mat.SetFloat("_Smoothness", usedMask ? 1f : (saved.Floats.TryGetValue("_Smoothness", out float s) ? s : 0.5f));

            // Emission
            Texture emissive = FirstTexture(saved, EmissiveMapNames);
            Color emissiveColor = saved.Colors.TryGetValue("_EmissiveColor", out var ec) ? ec : Color.black;
            if (emissive != null || emissiveColor.maxColorComponent > 0.001f)
            {
                if (emissive != null) mat.SetTexture("_EmissionMap", emissive);
                mat.SetColor("_EmissionColor", emissiveColor);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            // Transparency / alpha clipping
            bool transparent = saved.Floats.TryGetValue("_SurfaceType", out float st) && st >= 0.5f;
            bool alphaClip = saved.Floats.TryGetValue("_AlphaCutoffEnable", out float ace) && ace >= 0.5f;
            if (alphaClip)
            {
                mat.SetFloat("_AlphaClip", 1f);
                mat.SetFloat("_Cutoff", saved.Floats.TryGetValue("_AlphaCutoff", out float cut) ? cut : 0.5f);
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            }
            else if (transparent)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0f);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            if (!hasBase)
                report.AppendLine($"  NO BASE MAP: {mat.name} — converted but gray; assign its albedo texture by hand.");
            else if (baseMap != FirstTexture(saved, BaseMapNames))
                report.AppendLine($"  GUESSED base map for {mat.name}: '{baseMap.name}' — eyeball it.");

            return hasBase;
        }

        // URP samples metallic from R and smoothness from A of _MetallicGlossMap, and AO from G of
        // _OcclusionMap — HDRP packs all of it into one mask map, so unpack into two generated PNGs.
        static bool ApplyMaskMap(Material mat, Texture2D mask, StringBuilder report)
        {
            string maskPath = AssetDatabase.GetAssetPath(mask);
            if (string.IsNullOrEmpty(maskPath))
                return false;

            string dir = Path.GetDirectoryName(maskPath);
            string baseName = Path.GetFileNameWithoutExtension(maskPath);
            string metallicPath = $"{dir}/{baseName}_URP_MetallicSmoothness.png";
            string occlusionPath = $"{dir}/{baseName}_URP_Occlusion.png";

            if (!File.Exists(metallicPath))
            {
                Texture2D readable = ReadTexture(mask);
                Color32[] px = readable.GetPixels32();
                var metallicPx = new Color32[px.Length];
                var occlusionPx = new Color32[px.Length];
                for (int i = 0; i < px.Length; i++)
                {
                    metallicPx[i] = new Color32(px[i].r, px[i].r, px[i].r, px[i].a);
                    occlusionPx[i] = new Color32(px[i].g, px[i].g, px[i].g, 255);
                }
                WritePng(metallicPath, metallicPx, readable.width, readable.height);
                WritePng(occlusionPath, occlusionPx, readable.width, readable.height);
                Object.DestroyImmediate(readable);
                foreach (string p in new[] { metallicPath, occlusionPath })
                {
                    AssetDatabase.ImportAsset(p);
                    if (AssetImporter.GetAtPath(p) is TextureImporter imp)
                    {
                        imp.sRGBTexture = false;
                        imp.SaveAndReimport();
                    }
                }
                report.AppendLine($"  Unpacked mask map: {baseName} -> _URP_MetallicSmoothness + _URP_Occlusion");
            }

            var metallicTex = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicPath);
            var occlusionTex = AssetDatabase.LoadAssetAtPath<Texture2D>(occlusionPath);
            if (metallicTex == null)
                return false;

            mat.SetTexture("_MetallicGlossMap", metallicTex);
            mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            if (occlusionTex != null)
            {
                mat.SetTexture("_OcclusionMap", occlusionTex);
                mat.EnableKeyword("_OCCLUSIONMAP");
            }
            return true;
        }

        static Texture2D ReadTexture(Texture src)
        {
            var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(src, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false, true);
            tex.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return tex;
        }

        static void WritePng(string path, Color32[] pixels, int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
            tex.SetPixels32(pixels);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        static Texture FirstTexture(SavedProperties saved, string[] names)
        {
            foreach (string n in names)
                if (saved.Textures.TryGetValue(n, out Texture t) && t != null)
                    return t;
            return null;
        }

        // Last resort for custom shaders: any texture whose name smells like an albedo, else any non-normal/mask texture.
        static Texture GuessBaseMap(SavedProperties saved)
        {
            foreach (var kv in saved.Textures)
            {
                if (kv.Value == null) continue;
                string n = kv.Value.name.ToLowerInvariant();
                if (n.Contains("albedo") || n.Contains("basecolor") || n.Contains("base_color") || n.Contains("diffuse") || n.Contains("_col"))
                    return kv.Value;
            }
            foreach (var kv in saved.Textures)
            {
                if (kv.Value == null) continue;
                string n = kv.Value.name.ToLowerInvariant();
                if (!n.Contains("normal") && !n.Contains("mask") && !n.Contains("_nrm") && !n.Contains("_ao"))
                    return kv.Value;
            }
            return null;
        }

        class SavedProperties
        {
            public readonly Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();
            public readonly Dictionary<Texture, (Vector2, Vector2)> TexScaleOffset = new Dictionary<Texture, (Vector2, Vector2)>();
            public readonly Dictionary<string, float> Floats = new Dictionary<string, float>();
            public readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>();
        }

        // Reads the material's serialized property tables directly — works even when the shader is missing.
        static SavedProperties ReadSavedProperties(Material mat)
        {
            var result = new SavedProperties();
            var so = new SerializedObject(mat);

            SerializedProperty texEnvs = so.FindProperty("m_SavedProperties.m_TexEnvs");
            for (int i = 0; texEnvs != null && i < texEnvs.arraySize; i++)
            {
                SerializedProperty el = texEnvs.GetArrayElementAtIndex(i);
                string name = el.FindPropertyRelative("first").stringValue;
                var tex = el.FindPropertyRelative("second.m_Texture").objectReferenceValue as Texture;
                result.Textures[name] = tex;
                if (tex != null && !result.TexScaleOffset.ContainsKey(tex))
                    result.TexScaleOffset[tex] = (
                        el.FindPropertyRelative("second.m_Scale").vector2Value,
                        el.FindPropertyRelative("second.m_Offset").vector2Value);
            }

            SerializedProperty floats = so.FindProperty("m_SavedProperties.m_Floats");
            for (int i = 0; floats != null && i < floats.arraySize; i++)
            {
                SerializedProperty el = floats.GetArrayElementAtIndex(i);
                result.Floats[el.FindPropertyRelative("first").stringValue] = el.FindPropertyRelative("second").floatValue;
            }

            SerializedProperty colors = so.FindProperty("m_SavedProperties.m_Colors");
            for (int i = 0; colors != null && i < colors.arraySize; i++)
            {
                SerializedProperty el = colors.GetArrayElementAtIndex(i);
                result.Colors[el.FindPropertyRelative("first").stringValue] = el.FindPropertyRelative("second").colorValue;
            }

            return result;
        }
    }
}
