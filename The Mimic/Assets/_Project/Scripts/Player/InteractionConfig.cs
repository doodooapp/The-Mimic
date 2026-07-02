using UnityEngine;

namespace TheMimic
{
    // Tuning values for the player's crosshair interaction raycast.
    [CreateAssetMenu(menuName = "The Mimic/Interaction Config", fileName = "InteractionConfig")]
    public class InteractionConfig : ScriptableObject
    {
        [Min(0f)] public float interactRange = 3f;
        public LayerMask interactLayers = ~0;
    }
}
