using System;
using UnityEngine;

public class Pile : MonoBehaviour
{
    public PaperType pileType;
    public int paperCount;
    public event Action UpdateCount;
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
}
