using UnityEngine;

namespace TheMimic
{
    // Tuning values for run randomization: how many targets get picked per run.
    [CreateAssetMenu(menuName = "The Mimic/Run Config", fileName = "RunConfig")]
    public class RunConfig : ScriptableObject
    {
        [Min(1)] public int targetsPerRun = 3;
    }
}
