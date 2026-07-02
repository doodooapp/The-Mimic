using System.Collections.Generic;
using UnityEngine;

namespace TheMimic
{
    // Tracks whether the player is currently inside any hiding spot.
    public class PlayerHideState : MonoBehaviour
    {
        readonly HashSet<HidingSpot> spots = new HashSet<HidingSpot>();

        public bool IsHidden => spots.Count > 0;

        public void EnterSpot(HidingSpot spot) => spots.Add(spot);
        public void ExitSpot(HidingSpot spot) => spots.Remove(spot);
    }
}
