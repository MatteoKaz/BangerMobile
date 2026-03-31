
using System;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;



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
    [SerializeField] public float swipeStartTime = 0.3f;
    [SerializeField] public float maxswipeTime = 0.3f;
    bool hasRemove = false;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    

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
        float distance = delta.magnitude;
        delta = delta.normalized;
        Vector2 diagonalUpRight = (Vector3.up + Vector3.right).normalized;
        Vector2 diagonalUpLeft = (Vector3.up + Vector3.left).normalized;
        Vector2 diagonalLowLeft = (Vector3.down + Vector3.left).normalized;
        Vector2 straightUp = Vector3.up;
        float dotdiagRight = Vector3.Dot(delta, diagonalUpRight);
        float dotdiagLeft = Vector3.Dot(delta, diagonalUpLeft);
        float dotDiagLowLeft = Vector3.Dot(delta, diagonalLowLeft);

        float dotUp = Vector3.Dot(delta, straightUp);
        float dotLeft = Vector3.Dot(delta, Vector3.left);
        bool HasCall = false;
        if (hasRemove == false)
        {
            if (dotUp > 0.95f)
            {

                if (paperRef != null && HasCall == false)
                {
                    HasCall = true;
                    paperRef.MoveUpTuyaux();
                    audioEventDispatcher.PlayExclusiveAudio(AudioType.Swipe1); 
                }

            }
            if (dotdiagRight > 0.7f)
            {

                if (paperRef != null && HasCall == false)
                {
                    HasCall = true;
                    paperRef.MoveRightTuyaux();
                    audioEventDispatcher.PlayExclusiveAudio(AudioType.Swipe2); 
                }

            }

            /* if (dotDiagLowLeft > 0.7)
             {
                 if (paperRef != null && HasCall == false)
                 {
                     HasCall = true;
                     paperRef.MoveToPile();
                 }
             }


             /*if (dotLeft > 0.8f)
             {
                 if (paperRef != null && HasCall == false)
                 {

                     HasCall = true;

                     paperRef.MoveToPile();


                 }

             }*/


            if (dotdiagLeft > 0.9f)
            {

                if (paperRef != null && HasCall == false)
                {
                    HasCall = true;
                    paperRef.MoveLeftTuyaux();
                    audioEventDispatcher.PlayExclusiveAudio(AudioType.Swipe3); 
                }

            }
         
        }


    }

    public void SwipeForMenu()
    {
        Vector2 delta = endPosition - startPosition;
        float distance = delta.magnitude;
        delta = delta.normalized;
        float dotLeft = Vector3.Dot(delta, Vector3.left);

        if (Math.Abs(dotLeft) > 0.7f)
        {
            if (dotLeft > 0)
            {
                audioEventDispatcher.PlayExclusiveAudio(AudioType.TurnPageLeft);
                OnMoveLeft?.Invoke();
                
            }
            else
            {
                audioEventDispatcher.PlayExclusiveAudio(AudioType.TurnPageRight);
                OnMoveRight?.Invoke();
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
            if (paperRef != null) return;
            startPosition = touch.screenPosition;
            swipeStartTime = Time.time;
            debugText = "Touches: " + Touch.activeTouches.Count;
            Debug.Log("Start");

            // RaycastHit2D hit = Physics2D.Raycast(touchPos2D, Vector2.zero);
            Collider2D hit = Physics2D.OverlapPoint(touchPos2D);
            Debug.DrawRay(touchPos2D, Vector2.zero);
            if (hit != null)
            {
                Collider2D[] hits = Physics2D.OverlapPointAll(touchPos2D);

                PaperMove paper = hits
                    .Select(h => h.GetComponent<PaperMove>())
                    .Where(p => p != null)
                    .OrderByDescending(p => p.GetComponent<SpriteRenderer>().sortingOrder)
                    .FirstOrDefault();

                if (paper != null)
                {
                    paperRef = paper;

                    if (paperRef.OnPile)
                    {
                        paperRef.RemoveFromPile();
                        hasRemove = true;
                    }
                }




            
            }
            

        }
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            endPosition = touch.screenPosition;
            float swipeDuration = Time.time - swipeStartTime;
            if (swipeDuration > 0.1f) 
            {
                OnSwipe();
                SwipeForMenu();
            }
            hasRemove = false;
            paperRef = null;

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
