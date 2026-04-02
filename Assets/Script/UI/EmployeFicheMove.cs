using System.Collections;
using UnityEngine;

public class EmployeFicheMove : MonoBehaviour
{
    [SerializeField] InputPlayerManagerCustom inputPlayerManagerCustom;
    [SerializeField] GameObject Parent;
    [SerializeField] RectTransform content;
    [SerializeField] AnimationCurve swipeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float pageWidth = 1077f;
    public float animDuration = 0.25f;
    public int maxPage = 6;
    public float baseParentPose = 68f;

    [SerializeField] AudioEventDispatcher audioEventDispatcher;

    private int _currentPage = 0;
    private bool _isMoving = false;
    private bool _ficheReachedNotified = false;

    void OnEnable()
    {
        inputPlayerManagerCustom.OnMoveLeft  += LeftSwipe;
        inputPlayerManagerCustom.OnMoveRight += RightSwipe;
    }

    void OnDisable()
    {
        inputPlayerManagerCustom.OnMoveLeft  -= LeftSwipe;
        inputPlayerManagerCustom.OnMoveRight -= RightSwipe;
    }

    public void LeftSwipe()
    {
        if (!Parent.activeInHierarchy || _isMoving)
            return;

        if (audioEventDispatcher != null)
            audioEventDispatcher.PlayAudio(AudioType.TurnPageLeft);

        _currentPage = (_currentPage + 1) % (maxPage + 1);

        NotifyFicheReachedOnce();
        StartCoroutine(MoveToPage());
    }

    void RightSwipe()
    {
        if (!Parent.activeInHierarchy || _isMoving)
            return;

        if (audioEventDispatcher != null)
            audioEventDispatcher.PlayAudio(AudioType.TurnPageRight);

        _currentPage--;
        if (_currentPage < 0)
            _currentPage = maxPage;

        NotifyFicheReachedOnce();
        StartCoroutine(MoveToPage());
    }

    /// <summary>Notifie le TutorialManager la première fois que le joueur swipe dans les fiches employés.</summary>
    private void NotifyFicheReachedOnce()
    {
        if (_ficheReachedNotified) return;

        _ficheReachedNotified = true;
        TutorialManager.NotifyEmployeeFicheReached();
    }

    IEnumerator MoveToPage()
    {
        _isMoving = true;

        Vector2 startPos  = content.anchoredPosition;
        float baseX       = baseParentPose;
        Vector2 targetPos = new Vector2(baseX - _currentPage * pageWidth, startPos.y);

        float t = 0f;

        while (t < animDuration)
        {
            t += Time.deltaTime;
            float normalized = t / animDuration;
            float curve      = swipeCurve.Evaluate(normalized);
            content.anchoredPosition = Vector2.Lerp(startPos, targetPos, curve);
            yield return null;
        }

        content.anchoredPosition = targetPos;
        _isMoving = false;
    }
}
