using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheMimic
{
    // A scene light that can flicker violently and restore itself; registers so LightsController can reach it.
    [RequireComponent(typeof(Light))]
    public class FlickerableLight : MonoBehaviour
    {
        static readonly List<FlickerableLight> all = new List<FlickerableLight>();
        public static IReadOnlyList<FlickerableLight> All => all;

        Light cachedLight;
        float baseIntensity;
        Coroutine flickerRoutine;

        void Awake()
        {
            cachedLight = GetComponent<Light>();
            baseIntensity = cachedLight.intensity;
        }

        void OnEnable() => all.Add(this);

        void OnDisable()
        {
            all.Remove(this);
            StopFlicker();
        }

        public void StartFlicker(LightsConfig config)
        {
            if (config == null)
                return;
            StopFlicker();
            flickerRoutine = StartCoroutine(Flicker(config));
        }

        public void StopFlicker()
        {
            if (flickerRoutine != null)
            {
                StopCoroutine(flickerRoutine);
                flickerRoutine = null;
            }
            if (cachedLight != null)
                cachedLight.intensity = baseIntensity;
        }

        IEnumerator Flicker(LightsConfig config)
        {
            while (true)
            {
                cachedLight.intensity = baseIntensity * Random.Range(config.minIntensityMultiplier, config.maxIntensityMultiplier);
                yield return new WaitForSeconds(config.flickerInterval);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearRegistry() => all.Clear();
    }
}
