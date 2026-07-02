using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheMimic
{
    // Raises the phone while Tab is held and drains the battery only while it's raised; dead battery is dead for the run.
    public class PhoneController : MonoBehaviour
    {
        [SerializeField] PhoneConfig config;
        [SerializeField] InputActionReference phoneAction;
        [SerializeField] PhoneUI phoneUI;

        public event Action OnPhoneRaised;
        public event Action OnPhoneLowered;
        public event Action OnBatteryDead;

        public float BatteryPercent { get; private set; }
        public bool IsRaised { get; private set; }
        public bool IsDead => BatteryPercent <= 0f;

        void Awake()
        {
            BatteryPercent = config != null ? config.startPercent : 100f;

            if (config == null)
                Debug.LogError("[PhoneController] Config is not assigned. Assign a PhoneConfig asset in the Inspector.", this);
            if (phoneAction == null)
                Debug.LogError("[PhoneController] Phone Action is not assigned. Assign InputSystem_Actions > Player/Phone in the Inspector.", this);
            if (phoneUI == null)
                Debug.LogError("[PhoneController] Phone UI is not assigned. Assign the PhoneUI component in the Inspector.", this);
        }

        // No matching Disable(): the action belongs to the shared InputSystem_Actions asset,
        // so disabling it here would kill it for every other consumer. Update() stopping is enough.
        void OnEnable()
        {
            if (phoneAction != null)
                phoneAction.action.Enable();
        }

        void Update()
        {
            bool wantRaised = !IsDead && phoneAction != null && phoneAction.action.IsPressed();
            if (wantRaised != IsRaised)
                SetRaised(wantRaised);

            if (IsRaised && config != null)
            {
                BatteryPercent = Mathf.Max(0f, BatteryPercent - config.drainPerSecond * Time.deltaTime);
                if (phoneUI != null)
                    phoneUI.SetBattery(BatteryPercent);

                if (IsDead)
                {
                    SetRaised(false);
                    Debug.Log("[PhoneController] Battery dead — phone is gone for this run.", this);
                    OnBatteryDead?.Invoke();
                }
            }
        }

        void SetRaised(bool raised)
        {
            IsRaised = raised;
            if (phoneUI != null)
            {
                phoneUI.SetBattery(BatteryPercent);
                phoneUI.SetVisible(raised);
            }

            if (raised) OnPhoneRaised?.Invoke();
            else OnPhoneLowered?.Invoke();
        }
    }
}
