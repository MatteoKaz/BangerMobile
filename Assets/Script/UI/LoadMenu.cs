
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMenu : MonoBehaviour
{
    

    [SerializeField] private string mainMenuSceneName;
    [SerializeField] private string reloadLevel;


    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadSameScene()
    {
        SceneManager.LoadScene(reloadLevel);
    }
}
