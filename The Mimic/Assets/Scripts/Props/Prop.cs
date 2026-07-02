using System;
using UnityEngine;

namespace TheMimic
{
    // A physical object in the house that remembers where it started and announces when the player interacts with it.
    public class Prop : MonoBehaviour, IInteractable
    {
        public event Action<Prop> Interacted;

        public Vector3 HomePosition { get; private set; }
        public Quaternion HomeRotation { get; private set; }

        void Awake()
        {
            HomePosition = transform.position;
            HomeRotation = transform.rotation;
            PropRegistry.Register(this);
        }

        void OnDestroy()
        {
            PropRegistry.Unregister(this);
        }

        public void Interact()
        {
            Debug.Log($"[Prop] Interacted with '{name}'.", this);
            Interacted?.Invoke(this);
        }
    }
}
