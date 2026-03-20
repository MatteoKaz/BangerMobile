using TMPro;
using UnityEngine;

public class UiTimer : MonoBehaviour
{
    [SerializeField] TimeManager timeManager;
    [SerializeField] TextMeshProUGUI textMeshPro;

    public void Update()
    {
        if (timeManager != null)
        {
            textMeshPro.text = $"{timeManager.DayDurationToShow}";
        }
    }
}
