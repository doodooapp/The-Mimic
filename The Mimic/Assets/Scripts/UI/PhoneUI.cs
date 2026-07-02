using UnityEngine;
using UnityEngine.UI;

namespace TheMimic
{
    // Screen-space phone panel: grandma's three target photos and the battery readout.
    public class PhoneUI : MonoBehaviour
    {
        [SerializeField] GameObject panelRoot;
        [SerializeField] Text batteryText;
        [SerializeField] Image[] photoSlots = new Image[3];
        [SerializeField] Text[] photoLabels = new Text[3];

        void Awake()
        {
            if (panelRoot == null)
            {
                Debug.LogError("[PhoneUI] Panel Root is not assigned. Assign the PhonePanel GameObject in the Inspector.", this);
                return;
            }
            panelRoot.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            if (panelRoot != null)
                panelRoot.SetActive(visible);
        }

        public void SetBattery(float percent)
        {
            if (batteryText != null)
                batteryText.text = $"Battery {Mathf.CeilToInt(percent)}%";
        }

        // Placeholder "photo": a colored square with a label. RunDirector remaps these per run later.
        public void SetPhoto(int index, Color color, string label)
        {
            if (index < 0 || index >= photoSlots.Length)
                return;
            if (photoSlots[index] != null)
                photoSlots[index].color = color;
            if (index < photoLabels.Length && photoLabels[index] != null)
                photoLabels[index].text = label;
        }
    }
}
