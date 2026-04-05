using UnityEditor;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] UIScore uiscore;
    [SerializeField] GameObject GameOverHud;
    [SerializeField] SaveManager savemanager;
    [SerializeField] AudioEventDispatcher audioeventdispatcher;

    public void ShowGameOverHud()
    {
        audioeventdispatcher.PlayAudio(AudioType.GameOver);
      GameOverHud.SetActive(true);
       savemanager.DeleteSave();

    }


}
