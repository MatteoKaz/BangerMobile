using TMPro;
using UnityEngine;

public class UiTimer : MonoBehaviour
{
    [SerializeField] TimeManager timeManager;
    [SerializeField] TextMeshProUGUI textMeshPro;
    [SerializeField] PaperSpawner spawner;
    public void Update()
    {
        if (timeManager != null)
        {
            textMeshPro.text = $"{spawner.papersRemaining}";
        }
    }
}
