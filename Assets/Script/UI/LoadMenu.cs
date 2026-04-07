
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMenu : MonoBehaviour
{
    

    [SerializeField] private string mainMenuSceneName;
    [SerializeField] private string reloadLevel;


    public void LoadMainMenu()
    {
        MusicManager.Instance?.StopMenu();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadSameScene()
    {
        SceneManager.LoadScene(reloadLevel);
    }
}
