using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheMimic
{
    // One-click repair for the HDRP pack's blown-out lighting in URP: rescales the house prefab's
    // physical-unit light intensities, clamps HDR emission on converted materials, adds
    // FlickerableLight to every house light, and dims scene ambient/sun for horror mood.
    public static class HauntedHouseLightingFixer
    {
        const string HousePrefabPath = "Assets/GhostbuGaming/Vintage Haunted House/Arts/Prefab/VintageHauntedHouse.prefab";
        const string PackFolder = "Assets/GhostbuGaming";

        // HDRP physical intensities (hundreds/thousands) -> sane URP values.
        const float PhysicalThreshold = 20f;
        const float IntensityDivisor = 600f;
        const float MinIntensity = 0.8f;
        const float MaxIntensity = 3.5f;
        const float EmissionPeak = 1.5f;

        [MenuItem("The Mimic/Fix Haunted House Lighting")]
        public static void Fix()
        {
            if (!EditorUtility.DisplayDialog("Fix Haunted House Lighting",
                "This will:\n" +
                "• Rescale HDRP physical light intensities (795 / 3183 → ~1.3–3.5) in the house prefab and open scene\n" +
                "• Add FlickerableLight to every point/spot light in the house\n" +
                "• Clamp glowing (HDR emissive) materials to a sane level\n" +
                "• Dim scene ambient + directional light for horror mood\n\n" +
                "Run with your working scene open.",
                "Fix", "Cancel"))
                return;

            var report = new StringBuilder("[LightingFixer] Report:\n");

            FixHousePrefab(report);
            FixSceneLights(report);
            FixEmissiveMaterials(report);
            DimAmbient(report);

            AssetDatabase.SaveAssets();
            Debug.Log(report.ToString());
        }

        static void FixHousePrefab(StringBuilder report)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(HousePrefabPath) == null)
            {
                report.AppendLine($"  House prefab not found at {HousePrefabPath} — skipped.");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(HousePrefabPath);
            int fixedCount = 0, flickerCount = 0;
            foreach (Light light in root.GetComponentsInChildren<Light>(true))
            {
                if (RescaleIntensity(light))
                    fixedCount++;
                if (light.type != LightType.Directional && light.GetComponent<FlickerableLight>() == null)
                {
                    light.gameObject.AddComponent<FlickerableLight>();
                    flickerCount++;
                }
            }
            PrefabUtility.SaveAsPrefabAsset(root, HousePrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            report.AppendLine($"  House prefab: rescaled {fixedCount} light intensities, added FlickerableLight to {flickerCount} lights.");
        }

        static void FixSceneLights(StringBuilder report)
        {
            int fixedCount = 0;
            foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                // Lights inside the house prefab instance inherit the prefab fix above.
                var source = PrefabUtility.GetCorrespondingObjectFromSource(light);
                if (source != null && AssetDatabase.GetAssetPath(source) == HousePrefabPath)
                    continue;

                if (light.type == LightType.Directional)
                {
                    if (light.intensity > 0.3f)
                    {
                        report.AppendLine($"  Scene sun '{light.name}': intensity {light.intensity:0.##} -> 0.15 (moonlight). Delete it entirely for full darkness.");
                        light.intensity = 0.15f;
                        fixedCount++;
                    }
                    continue;
                }

                if (RescaleIntensity(light))
                    fixedCount++;
                if (light.GetComponent<FlickerableLight>() == null)
                    light.gameObject.AddComponent<FlickerableLight>();
            }
            if (fixedCount > 0)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            report.AppendLine($"  Open scene: adjusted {fixedCount} loose lights (house-prefab lights handled via the prefab).");
        }

        static bool RescaleIntensity(Light light)
        {
            if (light.intensity <= PhysicalThreshold)
                return false;
            light.intensity = Mathf.Clamp(light.intensity / IntensityDivisor, MinIntensity, MaxIntensity);
            return true;
        }

        static void FixEmissiveMaterials(StringBuilder report)
        {
            int clamped = 0, disabled = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { PackFolder }))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (mat == null || mat.shader == null || !mat.shader.name.StartsWith("Universal Render Pipeline"))
                    continue;
                if (!mat.IsKeywordEnabled("_EMISSION"))
                    continue;

                Color emission = mat.GetColor("_EmissionColor");
                float peak = emission.maxColorComponent;
                if (peak <= 0.001f)
                {
                    mat.DisableKeyword("_EMISSION");
                    disabled++;
                }
                else if (peak > EmissionPeak)
                {
                    mat.SetColor("_EmissionColor", emission * (EmissionPeak / peak));
                    report.AppendLine($"  Emission clamped: {mat.name} (peak {peak:0.#} -> {EmissionPeak}).");
                    clamped++;
                }
                EditorUtility.SetDirty(mat);
            }
            report.AppendLine($"  Materials: {clamped} HDR emissions clamped, {disabled} pointless emission flags removed.");
        }

        static void DimAmbient(StringBuilder report)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.06f, 0.065f, 0.08f);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            report.AppendLine("  Scene ambient set to flat dark (horror mood). Undo via Window > Rendering > Lighting > Environment.");
        }
    }
}
