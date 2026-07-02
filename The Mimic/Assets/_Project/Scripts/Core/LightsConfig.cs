using UnityEngine;

namespace TheMimic
{
    // Tuning values for the reveal light flicker: intensity band and how fast it jitters.
    [CreateAssetMenu(menuName = "The Mimic/Lights Config", fileName = "LightsConfig")]
    public class LightsConfig : ScriptableObject
    {
        [Min(0f)] public float minIntensityMultiplier = 0f;
        [Min(0f)] public float maxIntensityMultiplier = 1.6f;
        [Min(0.01f)] public float flickerInterval = 0.05f;
    }
}
