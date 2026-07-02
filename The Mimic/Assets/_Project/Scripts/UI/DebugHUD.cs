using UnityEngine;

namespace TheMimic
{
    // IMGUI debug overlay: battery, crosshair dot, and what the crosshair is aimed at.
    public class DebugHUD : MonoBehaviour
    {
        [SerializeField] PhoneController phone;
        [SerializeField] PlayerInteraction interaction;
        [SerializeField] ObjectiveManager objectives;
        [SerializeField] PlayerHideState hideState;
        [SerializeField] RunDirector runDirector;

        string seedInput = "";

        void OnGUI()
        {
            float y = 10f;

            if (phone != null)
            {
                string state = phone.IsDead ? " (DEAD)" : phone.IsRaised ? " (raised)" : "";
                GUI.Label(new Rect(10f, y, 500f, 22f), $"Battery: {Mathf.CeilToInt(phone.BatteryPercent)}%{state}");
                y += 22f;
            }

            if (objectives != null)
            {
                GUI.Label(new Rect(10f, y, 500f, 22f), $"Items: {objectives.CollectedCount}/{objectives.TotalCount}");
                y += 22f;
            }

            if (interaction != null && !string.IsNullOrEmpty(interaction.AimedName))
            {
                GUI.Label(new Rect(10f, y, 500f, 22f), $"[E] {interaction.AimedName}");
                y += 22f;
            }

            if (hideState != null && hideState.IsHidden)
            {
                GUI.Label(new Rect(10f, y, 500f, 22f), "HIDDEN");
                y += 22f;
            }

            GUI.DrawTexture(new Rect(Screen.width / 2f - 2f, Screen.height / 2f - 2f, 4f, 4f), Texture2D.whiteTexture);

            if (runDirector != null)
            {
                float bottom = Screen.height - 32f;
                GUI.Label(new Rect(10f, bottom, 220f, 22f), $"Seed: {runDirector.CurrentSeed}");
                seedInput = GUI.TextField(new Rect(230f, bottom, 110f, 22f), seedInput);
                if (GUI.Button(new Rect(345f, bottom, 140f, 22f), "Set seed + restart")
                    && int.TryParse(seedInput, out int seed))
                {
                    RunDirector.SetPendingSeed(seed);
                    if (GameManager.Instance != null)
                        GameManager.Instance.Restart();
                }
            }
        }
    }
}
