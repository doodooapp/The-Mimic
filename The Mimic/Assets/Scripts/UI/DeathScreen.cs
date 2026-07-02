using UnityEngine;

namespace TheMimic
{
    // End-of-run overlay: reveals which prop was the Mimic and shows run stats, replacing GameManager's bare overlay.
    public class DeathScreen : MonoBehaviour
    {
        [SerializeField] PhoneController phone;
        [SerializeField] ObjectiveManager objectives;
        [SerializeField] MimicController mimic;

        string statsText;

        void Awake()
        {
            if (phone == null)
                Debug.LogError("[DeathScreen] Phone is not assigned. Assign the PhoneController in the Inspector.", this);
            if (objectives == null)
                Debug.LogError("[DeathScreen] Objectives is not assigned. Assign the ObjectiveManager in the Inspector.", this);
            if (mimic == null)
                Debug.LogError("[DeathScreen] Mimic is not assigned. Assign the MimicController in the Inspector.", this);
        }

        void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SuppressDefaultOverlay = true;
        }

        void OnGUI()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == GameManager.RunState.Playing)
            {
                statsText = null; // stats are captured fresh once the run ends
                return;
            }

            if (statsText == null)
                statsText = BuildStats(gm);

            float w = 560f, h = 300f;
            var box = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(box, "");

            var title = new GUIStyle(GUI.skin.label) { fontSize = 36, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(box.x, box.y + 15f, w, 50f), gm.State == GameManager.RunState.Won ? "YOU ESCAPED" : "YOU DIED", title);

            var body = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.UpperCenter };
            GUI.Label(new Rect(box.x, box.y + 80f, w, h - 120f), statsText, body);

            var hint = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(box.x, box.y + h - 40f, w, 30f), "Press R to restart", hint);
        }

        string BuildStats(GameManager gm)
        {
            string mimicLine = mimic != null
                ? $"The Mimic was disguised as '{mimic.DisguisePropId}' at {mimic.DisguisePropPosition:F1}"
                : "The Mimic was... unknown (no MimicController assigned)";
            string items = objectives != null ? $"{objectives.CollectedCount}/{objectives.TotalCount}" : "?";
            string battery = phone != null ? $"{Mathf.CeilToInt(phone.BatteryPercent)}%" : "?";

            return $"{mimicLine}\n\nItems collected: {items}\nBattery remaining: {battery}\nTime survived: {gm.TimeSurvived:0.0}s";
        }
    }
}
