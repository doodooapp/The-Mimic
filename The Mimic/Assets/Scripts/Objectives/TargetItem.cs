using System;
using UnityEngine;

namespace TheMimic
{
    // Marks a Prop as one of grandma's belongings: interacting collects it (object disappears, propId announced).
    [RequireComponent(typeof(Prop))]
    public class TargetItem : MonoBehaviour
    {
        public static event Action<string> AnyCollected;

        [SerializeField] string propId;
        [SerializeField] Color photoColor = Color.white; // placeholder "photo" shown on the phone

        public string PropId => propId;
        public Color PhotoColor => photoColor;
        public bool IsCollected { get; private set; }

        // RunDirector turns this off for candidates that aren't part of this run's 3 targets.
        public bool IsActiveObjective { get; set; } = true;

        Prop prop;

        void Awake()
        {
            prop = GetComponent<Prop>();
            if (string.IsNullOrEmpty(propId))
            {
                propId = gameObject.name;
                Debug.LogWarning($"[TargetItem] Prop Id was empty — defaulting to GameObject name '{propId}'.", this);
            }
        }

        void OnEnable() => prop.Interacted += HandleInteracted;
        void OnDisable() => prop.Interacted -= HandleInteracted;

        void HandleInteracted(Prop _)
        {
            if (IsCollected || !IsActiveObjective)
                return;

            IsCollected = true;
            gameObject.SetActive(false);
            Debug.Log($"[TargetItem] Collected '{propId}'.", this);
            AnyCollected?.Invoke(propId);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticEvent() => AnyCollected = null;
    }
}
