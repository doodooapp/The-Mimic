using UnityEngine;

namespace TheMimic
{
    // IMGUI debug overlay: battery, crosshair dot, and what the crosshair is aimed at.
    public class DebugHUD : MonoBehaviour
    {
        [SerializeField] PhoneController phone;
        [SerializeField] PlayerInteraction interaction;
        [SerializeField] ObjectiveManager objectives;

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

            GUI.DrawTexture(new Rect(Screen.width / 2f - 2f, Screen.height / 2f - 2f, 4f, 4f), Texture2D.whiteTexture);
        }
    }
}
