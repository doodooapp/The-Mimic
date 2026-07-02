using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheMimic
{
    // Tracks which target propIds have been collected this run and announces when all of them are.
    public class ObjectiveManager : MonoBehaviour
    {
        [SerializeField] List<string> targetPropIds = new List<string>();

        public event Action<string, int> OnItemCollected; // propId, new collected count
        public event Action OnAllItemsCollected;

        readonly HashSet<string> collected = new HashSet<string>();

        public int CollectedCount => collected.Count;
        public int TotalCount => targetPropIds.Count;
        public bool AllCollected => TotalCount > 0 && collected.Count >= TotalCount;

        void Awake()
        {
            if (targetPropIds.Count == 0)
                Debug.LogError("[ObjectiveManager] No target propIds set. Add the 3 target ids in the Inspector.", this);
        }

        void OnEnable() => TargetItem.AnyCollected += HandleCollected;
        void OnDisable() => TargetItem.AnyCollected -= HandleCollected;

        // RunDirector replaces the Inspector targets with its random pick at run start.
        public void SetTargets(IEnumerable<string> ids)
        {
            targetPropIds = new List<string>(ids);
            collected.Clear();
        }

        void HandleCollected(string propId)
        {
            if (!targetPropIds.Contains(propId) || !collected.Add(propId))
                return;

            Debug.Log($"[ObjectiveManager] {collected.Count}/{TotalCount} items collected.", this);
            OnItemCollected?.Invoke(propId, collected.Count);

            if (AllCollected)
            {
                Debug.Log("[ObjectiveManager] All items collected — the exit door is unlocked.", this);
                OnAllItemsCollected?.Invoke();
            }
        }
    }
}
