using System;
using System.Collections;
using System.Data;
using TMPro;

using UnityEngine;
public class UIDay : MonoBehaviour
{
    [SerializeField] UiManager UiManager;
    [SerializeField] DayManager dayManager;
    [SerializeField] TextMeshProUGUI day;
    [SerializeField] TextMeshProUGUI week;
    [SerializeField] float timeAtScreenValue = 3f;
    public event Action EndShowing;
    public event Action LaunchFade;
    public event Action LaunchFadeIN;
    private void OnEnable()
    {
        //Ici le bug Lance a la suite le fade 
        UiManager.LaunchDayAnim += SetUpDayWeek;
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

    public IEnumerator TimeAtScreen()
    {

        yield return new WaitForSeconds(timeAtScreenValue);
        LaunchFade?.Invoke();

        yield return new WaitForSeconds(2.5f);
        EndShowing?.Invoke();

    }



}
