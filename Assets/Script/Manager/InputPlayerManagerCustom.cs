using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;


public class InputPlayerManagerCustom : MonoBehaviour
{
    public event Action OnMoveLeft;
    public event Action OnMoveRight;

    [SerializeField] private float _tapDuration = 1.0f;
    private float _tapTimer = 0.0f;
    private bool _isTouching = false;
    private float width = 0.0f;
    private float height = 0.0f;

    private Vector2 startPosition;
    string debugText = "";
    private Vector2 endPosition;
    [SerializeField] private Camera mainCamera;
    private PaperMove paperRef;
   

    void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        width = Screen.width;
        height = Screen.height;




    }

    //public void OnTap()
    //{
    //    Debug.Log("TAP");
    //}

    private void OnSwipe()
    {
        Vector2 delta = endPosition - startPosition;
        delta = delta.normalized;
        Vector2 diagonalUpRight = (Vector3.up + Vector3.right).normalized;
        Vector2 diagonalUpLeft = (Vector3.up + Vector3.left).normalized;
        Vector2 straightUp = Vector3.up;
        float dotdiagRight = Vector3.Dot(delta, diagonalUpRight);
        float dotdiagLeft = Vector3.Dot(delta, diagonalUpLeft);
        float dotUp = Vector3.Dot(delta, straightUp);
        float dotRight = Vector3.Dot(delta, Vector3.right);
        bool HasCall = false;

        if (dotdiagRight > 0.78f)
        {
           
                if (paperRef != null && HasCall == false)
                {
                    HasCall = true;
                    paperRef.MoveRightTuyaux();
                }
            
        }
        if (dotdiagLeft > 0.7f)
        {
          
                if (paperRef != null && HasCall == false)
                {
                     HasCall = true;
                    paperRef.MoveLeftTuyaux();
                }
           
        }
        if (dotUp > 0.85f)
        {
           
                if (paperRef != null && HasCall == false)
                {
                    HasCall = true;
                    paperRef.MoveUpTuyaux();
                }
            
        }
        if(Math.Abs(dotRight) > 0.8f)
        {
            if (paperRef != null && HasCall ==false)
            {
                HasCall = true;
                if (dotRight > 0)
                    paperRef.MoveToPile();
                else
                    paperRef.RemoveFromPile();
            }
          
        }
    }




    private void Update()
    {

        if (Touchscreen.current == null) return;
        if (Touch.activeTouches.Count == 0)
        {
            return;
        }
        var touch = Touch.activeTouches[0];

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(touch.screenPosition);
        Vector2 touchPos2D = new Vector2(worldPos.x, worldPos.y);

        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            startPosition = touch.screenPosition;
            debugText = "Touches: " + Touch.activeTouches.Count;
            Debug.Log("Start");

            RaycastHit2D hit = Physics2D.Raycast(touchPos2D, Vector2.zero);
            if (hit.collider != null)
            {
                PaperMove paper = hit.collider.GetComponent<PaperMove>();
                if (paper != null)
                {
                    paperRef = paper;

                }
            }
            

        }
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            endPosition = touch.screenPosition;
            OnSwipe();
            debugText = "Touchesend: " + Touch.activeTouches.Count;
            Debug.Log("end");
            paperRef = null;


        }

        Debug.Log(Touch.activeTouches.Count);




        //if (Input.touchCount > 0)
        //{
        //    Touch firstTouch = Input.GetTouch(0);

        //    if (firstTouch.phase == TouchPhase.Began)
        //    {
        //        _isTouching = true;
        //    }
        //    else if (firstTouch.phase == TouchPhase.Ended)
        //    {
        //        _isTouching = false;
        //        if (_tapTimer <= _tapDuration)
        //        {
        //            Debug.LogWarning($"Tap detected, Touch at {firstTouch.position}");

        //            if (firstTouch.position.x < width / 2)
        //            {
        //                Debug.LogWarning("Tap Right");
        //            }
        //            else
        //            {
        //                Debug.LogWarning("Tap Left");
        //            }
        //            _tapTimer = 0.0f;

        //        }

        //    }
        //    if (_isTouching)
        //    {
        //        _tapTimer += Time.deltaTime;
        //    }


        //    if (Input.GetKeyDown(KeyCode.RightArrow))
        //    {
        //        MoveRight();
        //    }

        //    if (Input.GetKeyDown(KeyCode.LeftArrow))
        //    {
        //        MoveLeft();

        //    }
        //}
    }
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 1500, 1500), debugText);
    }
    public void MoveDiagoRight()
    {
        OnMoveLeft?.Invoke();
    }

    public void MoveRight()
    {
        OnMoveRight?.Invoke();

    }
}
