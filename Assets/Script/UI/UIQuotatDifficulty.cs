using TMPro;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] QuotatManager quotatManager;
    [SerializeField] TextMeshProUGUI textMeshPro;
    public Type type;
    public enum Type
    {
        Easy,
        Medium,
        Hard,
    }

    public void OnEnable()
    {
        quotatManager.CalculQuotat += SetText;
    }
    public void OnDisable()
    {
        quotatManager.CalculQuotat -= SetText;

    }
    public void SetText()
    {
        switch(type)
        {
            case Type.Easy:
                textMeshPro.text = $"Quotat: {quotatManager.quotatEasy}";
                break;
            case Type.Medium:
                textMeshPro.text = $"Quotat: {quotatManager.quotatMid}";
                break;
            case Type.Hard:
                textMeshPro.text = $"Quotat: {quotatManager.quotatHard}";
                break;
        }
        
    }



}
    