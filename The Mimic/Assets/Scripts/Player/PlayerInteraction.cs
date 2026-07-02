using UnityEngine;
using UnityEngine.InputSystem;

namespace TheMimic
{
    // Raycasts from the camera crosshair, tracks what it's aimed at, and calls Interact() on it when the player presses E.
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] InteractionConfig config;
        [SerializeField] Camera playerCamera;
        [SerializeField] InputActionReference interactAction;

        // Name of the interactable under the crosshair, for the HUD prompt. Null when there is none.
        public string AimedName { get; private set; }

        IInteractable aimed;

        void Awake()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;

            if (config == null)
                Debug.LogError("[PlayerInteraction] Config is not assigned. Assign an InteractionConfig asset in the Inspector.", this);
            if (interactAction == null)
                Debug.LogError("[PlayerInteraction] Interact Action is not assigned. Assign InputSystem_Actions > Player > Interact in the Inspector.", this);
            if (playerCamera == null)
                Debug.LogError("[PlayerInteraction] No camera found. Assign the player camera or tag it MainCamera.", this);
        }

        // No matching Disable(): the action belongs to the shared InputSystem_Actions asset,
        // so disabling it here would kill it for every other consumer. Update() stopping is enough.
        void OnEnable()
        {
            if (interactAction != null)
                interactAction.action.Enable();
        }

        void Update()
        {
            UpdateAim();

            if (aimed != null && interactAction != null && interactAction.action.WasPressedThisFrame())
                aimed.Interact();
        }

        void UpdateAim()
        {
            aimed = null;
            AimedName = null;

            if (playerCamera == null || config == null)
                return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, config.interactRange, config.interactLayers, QueryTriggerInteraction.Ignore))
            {
                aimed = hit.collider.GetComponentInParent<IInteractable>();
                if (aimed is Component component)
                    AimedName = component.name;
            }
        }
    }
}
