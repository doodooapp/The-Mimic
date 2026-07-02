using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheMimic
{
    // Seeded run randomizer: at scene start, picks this run's target items from the candidate
    // pool, picks the Mimic's fake prop, and updates the phone photos to match.
    public class RunDirector : MonoBehaviour
    {
        [SerializeField] RunConfig config;
        [SerializeField] int inspectorSeed = 0;
        [SerializeField] bool randomizeSeedEachRun = true;
        [SerializeField] List<TargetItem> targetCandidates = new List<TargetItem>();
        [SerializeField] List<Prop> fakeCandidates = new List<Prop>();
        [SerializeField] ObjectiveManager objectives;
        [SerializeField] MimicController mimic;
        [SerializeField] PhoneUI phoneUI;

        public int CurrentSeed { get; private set; }

        // Set via the DebugHUD seed field; survives scene reloads so two players can
        // compare identical runs. Sticky until replaced or the editor session restarts.
        static int? pendingSeedOverride;
        public static void SetPendingSeed(int seed) => pendingSeedOverride = seed;

        void Awake()
        {
            if (config == null)
                Debug.LogError("[RunDirector] Config is not assigned. Assign a RunConfig asset in the Inspector.", this);
            if (objectives == null)
                Debug.LogError("[RunDirector] Objectives is not assigned.", this);
            if (mimic == null)
                Debug.LogError("[RunDirector] Mimic is not assigned.", this);
            if (phoneUI == null)
                Debug.LogError("[RunDirector] Phone UI is not assigned.", this);
        }

        void Start()
        {
            int targetsPerRun = config != null ? config.targetsPerRun : 3;
            if (targetCandidates.Count < targetsPerRun)
            {
                Debug.LogError($"[RunDirector] Need at least {targetsPerRun} Target Candidates, have {targetCandidates.Count}. Fill the list in the Inspector.", this);
                return;
            }
            if (fakeCandidates.Count == 0)
            {
                Debug.LogError("[RunDirector] Fake Candidates is empty. Add at least one fake Prop in the Inspector.", this);
                return;
            }

            CurrentSeed = pendingSeedOverride ?? (randomizeSeedEachRun ? Environment.TickCount : inspectorSeed);
            var rng = new System.Random(CurrentSeed);

            // Fisher-Yates shuffle, take the first N as this run's targets.
            var shuffled = new List<TargetItem>(targetCandidates);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            var chosenIds = new List<string>();
            for (int i = 0; i < shuffled.Count; i++)
            {
                if (shuffled[i] == null)
                    continue;
                bool isTarget = chosenIds.Count < targetsPerRun;
                shuffled[i].IsActiveObjective = isTarget;
                if (!isTarget)
                    continue;

                chosenIds.Add(shuffled[i].PropId);
                if (phoneUI != null)
                    phoneUI.SetPhoto(chosenIds.Count - 1, shuffled[i].PhotoColor, shuffled[i].PropId);
            }

            if (objectives != null)
                objectives.SetTargets(chosenIds);

            Prop fake = fakeCandidates[rng.Next(fakeCandidates.Count)];
            if (mimic != null && fake != null)
                mimic.AssignFakeProp(fake);

            Debug.Log($"[RunDirector] Seed {CurrentSeed}: targets [{string.Join(", ", chosenIds)}], fake '{(fake != null ? fake.name : "none")}'.", this);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticState() => pendingSeedOverride = null;
    }
}
