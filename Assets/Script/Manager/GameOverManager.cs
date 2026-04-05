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
      GameOverHud.SetActive(true); 
      audioeventdispatcher.PlayAudio(AudioType.GameOver);
       savemanager.DeleteSave();

    }


}
