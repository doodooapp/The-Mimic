using System.Collections.Generic;
using UnityEngine;

namespace TheMimic
{
    // Static list of every Prop currently alive in the scene.
    public static class PropRegistry
    {
        static readonly List<Prop> props = new List<Prop>();

        public static IReadOnlyList<Prop> All => props;

        public static void Register(Prop prop)
        {
            if (!props.Contains(prop))
                props.Add(prop);
        }

        public static void Unregister(Prop prop)
        {
            props.Remove(prop);
        }

        // Keeps the list clean when Enter Play Mode runs without a domain reload.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearOnPlayModeStart()
        {
            props.Clear();
        }
    }
}
