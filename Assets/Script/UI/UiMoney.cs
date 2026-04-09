using TMPro;
using UnityEngine;

public class UiMoney : MonoBehaviour
{
   [SerializeField] ScoreManager scoreManager;
    [SerializeField] TextMeshProUGUI text;
    private void Update()
    {
        text.text = $"Argent: {scoreManager.playerMoney}$";
    }
}
