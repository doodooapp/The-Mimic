using UnityEngine;

namespace TheMimic
{
    // Tuning values for the Mimic: reveal pause, hunt duration, detection cone, speeds, kill distance.
    [CreateAssetMenu(menuName = "The Mimic/Mimic Config", fileName = "MimicConfig")]
    public class MimicConfig : ScriptableObject
    {
        [Header("Timing")]
        [Min(0f)] public float revealPauseSeconds = 1.5f;
        [Min(1f)] public float huntDuration = 20f;

        [Header("Detection")]
        [Range(0f, 360f)] public float viewAngle = 110f;
        [Min(0f)] public float viewRange = 15f;
        [Min(0f)] public float eyeHeight = 1.6f;
        public LayerMask lineOfSightMask = ~0;

        [Header("Movement")]
        [Min(0f)] public float patrolSpeed = 2f;
        [Min(0f)] public float pursueSpeed = 4.5f;
        [Min(0f)] public float retreatSpeed = 3f;

        [Header("Kill")]
        [Min(0f)] public float killDistance = 1.2f;
    }
}
