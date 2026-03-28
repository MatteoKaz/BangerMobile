using System;

using UnityEngine;

public class ClosePopUp : MonoBehaviour
{
    [SerializeField] GameObject popup;
    public event Action ClosePopUpCallback;
    public void ClosePopUpall()
    {
        ClosePopUpCallback?.Invoke();
        popup.SetActive(false);
    }
}
