using UnityEditor;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] UIScore uiscore;
    [SerializeField] GameObject GameOverHud;
    [SerializeField] SaveManager savemanager;


    public void ShowGameOverHud()
    {
        GameOverHud.SetActive(true);
        savemanager.DeleteSave();

    }


}
