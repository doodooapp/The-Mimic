using UnityEngine;

namespace TheMimic
{
    // Flickers every registered FlickerableLight while the Mimic hunts and restores them when it retreats.
    public class LightsController : MonoBehaviour
    {
        [SerializeField] LightsConfig config;

        void Awake()
        {
            if (config == null)
                Debug.LogError("[LightsController] Config is not assigned. Assign a LightsConfig asset in the Inspector.", this);
        }

        void OnEnable()
        {
            MimicController.OnMimicRevealed += HandleRevealed;
            MimicController.OnMimicRetreating += HandleRetreating;
        }

        void OnDisable()
        {
            MimicController.OnMimicRevealed -= HandleRevealed;
            MimicController.OnMimicRetreating -= HandleRetreating;
        }

        void HandleRevealed()
        {
            for (int i = 0; i < FlickerableLight.All.Count; i++)
                FlickerableLight.All[i].StartFlicker(config);
        }

        void HandleRetreating()
        {
            for (int i = 0; i < FlickerableLight.All.Count; i++)
                FlickerableLight.All[i].StopFlicker();
        }
    }
}
