using UnityEngine;

namespace TheMimic
{
    // Tuning values for player movement feel: walk/sprint speeds, crouch, and look sensitivity/smoothing.
    [CreateAssetMenu(menuName = "The Mimic/Player Config", fileName = "PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        [Min(0f)] public float walkSpeed = 2.2f;
        [Tooltip("A/B toggle for playtests — design default is OFF (no easy escape from the chase).")]
        public bool sprintEnabled = false;
        [Min(0f)] public float sprintSpeed = 4.5f;

        [Header("Crouch (hold Ctrl)")]
        [Range(0.3f, 0.9f)] public float crouchHeightMultiplier = 0.5f;
        [Range(0.1f, 1f)] public float crouchSpeedMultiplier = 0.5f;

        [Header("Look")]
        [Min(0f)] public float lookSensitivity = 1f;
        [Tooltip("Seconds the camera takes to catch up to the mouse. 0 = raw, no smoothing.")]
        [Min(0f)] public float lookSmoothing = 0f;
    }
}
