using System.Collections;
using UnityEngine;

public class EmployeFicheMove : MonoBehaviour
{
    [SerializeField] InputPlayerManagerCustom inputPlayerManagerCustom;
    [SerializeField] GameObject Parent;
    [SerializeField] RectTransform content; // UI  RectTransform
    [SerializeField] AnimationCurve swipeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float pageWidth = 1077f;
    public float animDuration = 0.25f;
    public int maxPage = 6;
    public float baseParentPose = 68f;

    int currentPage = 0;
    bool isMoving = false;

    void OnEnable()
    {
        inputPlayerManagerCustom.OnMoveLeft += LeftSwipe;
        inputPlayerManagerCustom.OnMoveRight += RightSwipe;
    }

    void OnDisable()
    {
        inputPlayerManagerCustom.OnMoveLeft -= LeftSwipe;
        inputPlayerManagerCustom.OnMoveRight -= RightSwipe;
    }

    void LeftSwipe()
    {
        if (!Parent.activeInHierarchy || isMoving)
            return;

        currentPage = (currentPage + 1) % (maxPage + 1);

        StartCoroutine(MoveToPage());
    }

    void RightSwipe()
    {
        if (!Parent.activeInHierarchy || isMoving)
            return;

        currentPage--;
        if (currentPage < 0)
            currentPage = maxPage;

        StartCoroutine(MoveToPage());
    }

    IEnumerator MoveToPage()
    {
        isMoving = true;

        Vector2 startPos = content.anchoredPosition;
        float baseX = baseParentPose;

        Vector2 targetPos = new Vector2(baseX - currentPage * pageWidth, startPos.y);

        float t = 0f;

        

        

        while (t < animDuration)
        {
            t += Time.deltaTime;
            float normalized = t / animDuration;

            float curve = swipeCurve.Evaluate(normalized);
            content.anchoredPosition = Vector2.Lerp(startPos, targetPos, curve);

            yield return null;
        }

        content.anchoredPosition = targetPos;
        isMoving = false;
    }
}