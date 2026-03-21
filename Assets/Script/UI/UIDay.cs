using System;
using System.Collections;
using System.Data;
using TMPro;

using UnityEngine;
public class UIDay : MonoBehaviour
{
    [SerializeField] DayManager dayManager;
    [SerializeField] TextMeshProUGUI day;
    [SerializeField] TextMeshProUGUI week;
    [SerializeField] float timeAtScreenValue = 3f;
    public event Action EndShowing;
    public event Action LaunchFade;

    private void OnEnable()
    {
        //Ici le bug Lance a la suite le fade 
        dayManager.DayBegin += SetUpDayWeek;
    }
    private void OnDisable()
    {
        dayManager.DayBegin -= SetUpDayWeek;
    }
    public void SetUpDayWeek()
    {
        day.text = $"{dayManager.DayName}";
        week.text = $"Semaine: {dayManager.currentWeek}";

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
