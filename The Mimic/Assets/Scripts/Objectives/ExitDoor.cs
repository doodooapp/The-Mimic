using UnityEngine;

namespace TheMimic
{
    // The door the player interacts with to win — locked until every target item is collected.
    public class ExitDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] ObjectiveManager objectives;

        void Awake()
        {
            if (objectives == null)
                Debug.LogError("[ExitDoor] Objectives is not assigned. Assign the ObjectiveManager in the Inspector.", this);
        }

        public void Interact()
        {
            if (objectives == null)
                return;

            if (!objectives.AllCollected)
            {
                Debug.Log($"[ExitDoor] Locked — {objectives.CollectedCount}/{objectives.TotalCount} items collected.", this);
                return;
            }

            if (GameManager.Instance != null)
                GameManager.Instance.Win();
            else
                Debug.LogError("[ExitDoor] No GameManager in the scene.", this);
        }
    }
}
