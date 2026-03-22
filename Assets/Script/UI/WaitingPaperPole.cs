using TMPro;
using UnityEngine;

public class WaitingPaperPole : MonoBehaviour
{
    [SerializeField] Pole pole;
    [SerializeField] TextMeshProUGUI textPaper;

    public void OnEnable()
    {
        pole.UpdatePaperCount += TextUpdate;
    }
    public void OnDisable()
    {
        pole.UpdatePaperCount -= TextUpdate;
    }
    public void TextUpdate()
    {
        textPaper.text = $"{pole.activepaper}/{pole.totalPaper}";
    }
}
