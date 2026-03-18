using System;
using UnityEngine;

public class Tuyaux : MonoBehaviour
{
  
    public PaperType tuyauxType;
    public event Action AddPaper;

    public void GoodPaper()
    {
        Debug.Log("+1");
        AddPaper?.Invoke();
    }
}
