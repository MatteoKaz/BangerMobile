
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;


public class Swat : MonoBehaviour
{

    [SerializeField] Pole linkedPole;
    [SerializeField] SwatManager swatManager;
    [SerializeField] Button button;
    
    [SerializeField] Light2D light;

    void OnEnable()
    {
        swatManager.SwatModeStart += Show;
        swatManager.SwatModeEnd += Hide;
    }
    void OnDisable()
    {
        swatManager.SwatModeStart -= Show;
        swatManager.SwatModeEnd -= Hide;
    }

    void Show()
    {
        button.interactable = true;
        button.gameObject.GetComponent<Image>().enabled = true;

    }

    void Hide()
    {
        button.interactable = false;
        button.gameObject.GetComponent<Image>().enabled = false;
    }

    public void OnClick()
    {
        swatManager.light = light;
        swatManager.OnPoleSelected(linkedPole);
    }
     

}
