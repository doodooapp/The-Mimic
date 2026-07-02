using UnityEngine;
using UnityEngine.InputSystem;

namespace TheMimic
{
    // Raycasts from the camera crosshair and calls Interact() on whatever the player presses E on.
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] InteractionConfig config;
        [SerializeField] Camera playerCamera;
        [SerializeField] InputActionReference interactAction;

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

        void OnEnable()
        {
            if (interactAction != null)
                interactAction.action.Enable();
        }

        void OnDisable()
        {
            if (interactAction != null)
                interactAction.action.Disable();
        }

        void Update()
        {
            if (interactAction != null && interactAction.action.WasPressedThisFrame())
                TryInteract();
        }

        void TryInteract()
        {
            if (playerCamera == null || config == null)
                return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, config.interactRange, config.interactLayers, QueryTriggerInteraction.Ignore))
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                interactable?.Interact();
            }
        }
    }
}
