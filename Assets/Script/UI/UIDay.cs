using UnityEngine;
using TMPro;
using System.Data;
using System.Collections;
using System;
public class UIDay : MonoBehaviour
{
    [SerializeField] DayManager dayManager;
    [SerializeField] TextMeshProUGUI day;
    [SerializeField] TextMeshProUGUI week;

    public event Action EndShowing;

    private void OnEnable()
    {
        dayManager.DayBegin += SetUpDayWeek;
    }
    private void OnDisable()
    {
        dayManager.DayBegin -= SetUpDayWeek;
    }
    public void SetUpDayWeek()
    {
        day.text = $"{dayManager.DayName}";
        week.text = $"Semaine :{dayManager.currentWeek}";
        StartCoroutine(TimeAtScreen());

    }

    public IEnumerator TimeAtScreen()
    {
        yield return new WaitForSeconds(2f);
        EndShowing?.Invoke();

    }



}
