using System;
using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
public class UIDay : MonoBehaviour
{
    [SerializeField] UiManager UiManager;
    [SerializeField] DayManager dayManager;
    [SerializeField] TextMeshProUGUI day;
    [SerializeField] TextMeshProUGUI week;
    [SerializeField] Image dayFond;
    [SerializeField] float timeAtScreenValue = 3f;
    public event Action EndShowing;
    public event Action LaunchFade;
    public event Action LaunchFadeIN;

    private void OnEnable()
    {
        //Ici le bug Lance a la suite le fade 
        UiManager.LaunchDayAnim += SetUpDayWeek;
        UiManager.dayResetOpacity += ResetOpacity;
    }
    private void OnDisable()
    {
        UiManager.LaunchDayAnim -= SetUpDayWeek;
    }
    public void SetUpDayWeek()
    {
        day.text = $"{dayManager.DayName}";
        week.text = $"Semaine: {dayManager.currentWeek}";
        LaunchFadeIN?.Invoke();
        StartCoroutine(TimeAtScreen());

    }
    public void ResetOpacity()
    {
        dayFond.color = new Color(dayFond.color.r, dayFond.color.g, dayFond.color.b, 1f);
    }
    public IEnumerator TimeAtScreen()
    {

        yield return new WaitForSeconds(timeAtScreenValue);
        LaunchFade?.Invoke();

        yield return new WaitForSeconds(2.75f);
        EndShowing?.Invoke();

    }



}
