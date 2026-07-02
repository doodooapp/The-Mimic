using UnityEngine;

namespace TheMimic
{
    // A trigger volume (closet, under-bed) that flags the player as hidden while inside it.
    public class HidingSpot : MonoBehaviour
    {
        void Awake()
        {
            var col = GetComponent<Collider>();
            if (col == null || !col.isTrigger)
                Debug.LogError("[HidingSpot] Needs a Collider with 'Is Trigger' checked.", this);
        }

        void OnTriggerEnter(Collider other)
        {
            var hideState = other.GetComponentInParent<PlayerHideState>();
            if (hideState != null)
            {
                hideState.EnterSpot(this);
                Debug.Log($"[HidingSpot] Player hidden in '{name}'.", this);
            }
        }

        void OnTriggerExit(Collider other)
        {
            var hideState = other.GetComponentInParent<PlayerHideState>();
            if (hideState != null)
                hideState.ExitSpot(this);
        }
    }
}
