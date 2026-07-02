using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheMimic
{
    // Applies PlayerConfig on top of the Starter Assets FirstPersonController every frame:
    // slower walk, no jump, sprint gated by config, hold-Ctrl crouch, look sensitivity/smoothing.
    // Deliberately does NOT modify the Starter Assets scripts — it drives their public fields.
    [RequireComponent(typeof(FirstPersonController))]
    public class PlayerMovementTuner : MonoBehaviour
    {
        [SerializeField] PlayerConfig config;
        [SerializeField] InputActionReference crouchAction;
        [SerializeField] Transform cameraRoot; // the PlayerCameraRoot child

        public bool IsCrouched { get; private set; }

        FirstPersonController fpc;
        StarterAssetsInputs inputs;
        CharacterController controller;
        float standHeight;
        float controllerBottom;
        float standCameraY;
        Vector2 smoothedLook;

        void Awake()
        {
            fpc = GetComponent<FirstPersonController>();
            inputs = GetComponent<StarterAssetsInputs>();
            controller = GetComponent<CharacterController>();
            standHeight = controller.height;
            controllerBottom = controller.center.y - controller.height / 2f;
            if (cameraRoot != null)
                standCameraY = cameraRoot.localPosition.y;

            if (config == null)
                Debug.LogError("[PlayerMovementTuner] Config is not assigned. Assign a PlayerConfig asset in the Inspector.", this);
            if (crouchAction == null)
                Debug.LogError("[PlayerMovementTuner] Crouch Action is not assigned. Assign InputSystem_Actions > Player/Crouch in the Inspector.", this);
            if (cameraRoot == null)
                Debug.LogError("[PlayerMovementTuner] Camera Root is not assigned. Assign the PlayerCameraRoot child in the Inspector.", this);
        }

        // No matching Disable(): the action belongs to the shared InputSystem_Actions asset,
        // so disabling it here would kill it for every other consumer.
        void OnEnable()
        {
            if (crouchAction != null)
                crouchAction.action.Enable();
        }

        void Update()
        {
            if (config == null)
                return;

            // Jump is disabled for the prototype: swallow the input before the controller consumes it.
            if (inputs != null)
                inputs.jump = false;

            UpdateCrouch();

            float speedScale = IsCrouched ? config.crouchSpeedMultiplier : 1f;
            fpc.MoveSpeed = config.walkSpeed * speedScale;
            // Sprint's code path stays alive; with sprint disabled it just moves at walk speed.
            fpc.SprintSpeed = (config.sprintEnabled ? config.sprintSpeed : config.walkSpeed) * speedScale;
            fpc.RotationSpeed = config.lookSensitivity;

            if (inputs != null && config.lookSmoothing > 0f)
            {
                float blend = 1f - Mathf.Exp(-Time.deltaTime / config.lookSmoothing);
                smoothedLook = Vector2.Lerp(smoothedLook, inputs.look, blend);
                inputs.look = smoothedLook;
            }
        }

        void UpdateCrouch()
        {
            bool wantCrouch = crouchAction != null && crouchAction.action.IsPressed();
            if (wantCrouch != IsCrouched)
            {
                // Don't stand up into the bed slab — stay crouched until there's headroom.
                if (wantCrouch || HasHeadroom())
                {
                    IsCrouched = wantCrouch;
                    float height = standHeight * (IsCrouched ? config.crouchHeightMultiplier : 1f);
                    controller.height = height;
                    Vector3 center = controller.center;
                    center.y = controllerBottom + height / 2f;
                    controller.center = center;
                }
            }

            if (cameraRoot != null)
            {
                float targetY = standCameraY * (IsCrouched ? config.crouchHeightMultiplier : 1f);
                Vector3 p = cameraRoot.localPosition;
                p.y = Mathf.Lerp(p.y, targetY, Time.deltaTime * 12f);
                cameraRoot.localPosition = p;
            }
        }

        bool HasHeadroom()
        {
            float rayStart = controller.height - 0.05f;
            float rayLength = standHeight - controller.height + 0.15f;
            float r = controller.radius * 0.8f;
            Vector3[] offsets =
            {
                Vector3.zero,
                new Vector3(r, 0f, 0f), new Vector3(-r, 0f, 0f),
                new Vector3(0f, 0f, r), new Vector3(0f, 0f, -r),
            };
            foreach (Vector3 offset in offsets)
            {
                Vector3 origin = transform.position + Vector3.up * rayStart + offset;
                if (Physics.Raycast(origin, Vector3.up, rayLength, ~0, QueryTriggerInteraction.Ignore))
                    return false;
            }
            return true;
        }
    }
}
