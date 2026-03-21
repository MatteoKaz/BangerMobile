using System;
using UnityEngine;

public class Pile : MonoBehaviour
{
    public PaperType pileType;
    public int paperCount;
    public event Action UpdateCount;
    [SerializeField] DayManager dayManager;


    public void OnEnable()
    {
        dayManager.DayTransition += RemoveAll;
    }
    public void OnDisable()
    {
        dayManager.DayTransition -= RemoveAll;
    }
    public void AddToPile()
    {
        paperCount += 1;
        UpdateCount?.Invoke();

    }

    public void RemoveFromPile()
    {
        paperCount -= 1;
        UpdateCount?.Invoke();

    }

    public void RemoveAll()
    {
        paperCount = 0;
        UpdateCount?.Invoke();
    }
}
