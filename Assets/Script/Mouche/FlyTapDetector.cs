using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Détecte le tap du joueur sur la mouche au moment de l'appui.
/// Tap direct sur le collider  → OnTapped() → mouche écrasée.
/// Tap dans _fearRadius        → Flee()     → mouche fuit.
/// </summary>
[RequireComponent(typeof(FlyController), typeof(Collider2D))]
public class FlyTapDetector : MonoBehaviour
{
    [Header("Fuite")]
    [SerializeField] private float _fearRadius = 1.2f;

    private FlyController _flyController;
    private Collider2D _collider;
    private Camera _mainCamera;
    [SerializeField] AudioEventDispatcher audioEventDispatcher;

    private void Awake()
    {
        _flyController = GetComponent<FlyController>();
        _collider = GetComponent<Collider2D>();
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        audioEventDispatcher?.PlayLoopAudio(AudioType.FlyMouche);
    }

    private void Update()
    {
        CheckMouseInput();
        CheckTouchInput();
    }

    /// <summary>Gère le clic souris (éditeur / desktop).</summary>
    private void CheckMouseInput()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            TryTap(worldPos);
        }
    }

    /// <summary>Gère le tap tactile (mobile).</summary>
    private void CheckTouchInput()
    {
        if (Touchscreen.current == null) return;

        foreach (var touch in Touch.activeTouches)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(touch.screenPosition);
                TryTap(worldPos);
            }
        }
    }

    /// <summary>
    /// Tap sur le collider → écrase la mouche.
    /// Tap dans le rayon de peur → fuite.
    /// </summary>
    private void TryTap(Vector2 worldPosition)
    {
        if (_collider.OverlapPoint(worldPosition))
        {
            audioEventDispatcher?.StopLoopAudio();         
            audioEventDispatcher?.PlayAudio(AudioType.MoucheDead);
            _flyController.OnTapped();
            return;
        }

        float distance = Vector2.Distance(worldPosition, transform.position);
        if (distance <= _fearRadius)
        {
            _flyController.Flee(worldPosition);
        }
    }
    

    private void OnDestroy()
    {
        audioEventDispatcher?.StopLoopAudio();  
        EnhancedTouchSupport.Disable();
    }
}
