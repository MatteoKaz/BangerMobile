using UnityEngine;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isHeld = false;
    private float timeHeld = 0.3f;
    private float timer = 0f;
    private bool isOpen = false;

    [SerializeField] EmployeLink employeLink;
    [SerializeField] GameObject popup;
    public void OnPointerDown(PointerEventData eventData)
    {
        isHeld = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHeld = false;
        timer = 0f;
    }

    void Update()
    {
        if (isHeld)
        {
            
            timer += Time.deltaTime;
            if (timer > timeHeld)
            {
                openPopUp();
            }
        }
    }

    public void openPopUp()
    {
       
        if (isOpen)
        {
            isOpen = true;
            popup.SetActive(true);

        }
    }
}