using UnityEngine;

namespace TheMimic
{
    // Tuning values for the phone: starting battery and how fast it drains while raised.
    [CreateAssetMenu(menuName = "The Mimic/Phone Config", fileName = "PhoneConfig")]
    public class PhoneConfig : ScriptableObject
    {
        [Range(0f, 100f)] public float startPercent = 100f;
        [Min(0f)] public float drainPerSecond = 4f;
    }
}
