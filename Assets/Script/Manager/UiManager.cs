using UnityEngine;

public class UiManager : MonoBehaviour
{
    [SerializeField] DayManager dayManager;
    [SerializeField] QuotatManager quotatManager;

    [SerializeField] GameObject Day;
    [SerializeField] GameObject DifficultyChoice;
    [SerializeField] UIDay dayScript;



    private void OnEnable()
    {
        quotatManager.QuotatIsSet += DisableDifficultyUI;
        dayManager.DayBegin += EnableDay;
        dayScript.EndShowing += DisableDay;
    }
    private void OnDisable()
    {
        quotatManager.QuotatIsSet -= DisableDifficultyUI;
        dayManager.DayBegin -= EnableDay;
        dayScript.EndShowing -= DisableDay;
    }


    public void DisableDifficultyUI()
    {
        DifficultyChoice.SetActive(false);
    }

    public void EnableDay()
    {
        Day.SetActive(true);
    }

    public void DisableDay()
    {
        Day.SetActive(false);
    }
}
