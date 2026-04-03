using TMPro;
using UnityEngine;

public class PaperTime : MonoBehaviour
{
    [SerializeField] PaperMove paperref;
    [SerializeField] TextMeshProUGUI time;

    // Update is called once per frame
    void Update()
    {
        time.text = $"{Mathf.RoundToInt(paperref.Paperduration)}s";
    }
}
