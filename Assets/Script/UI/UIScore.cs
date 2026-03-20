using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class UIScore : MonoBehaviour
{
    [SerializeField] UiManager uiManager;
    [SerializeField] TextMeshProUGUI UiScore;
    [SerializeField] ScoreManager scoreManager;


    private void OnEnable()
    {
        uiManager.ScoreAnim += LaunchAnim;
    }

    private void OnDisable()
    {
        uiManager.ScoreAnim -= LaunchAnim;
    }

    public void LaunchAnim()
    {
        StartCoroutine(ScoreAnim());
    }

    public IEnumerator ScoreAnim()
    {
        int displayScore = 0;
        int score = scoreManager.playerQuotat;
        while (displayScore < score)
        {
            int step = Mathf.Clamp(displayScore / 100, 1, 300);
            displayScore += step;

            if (displayScore > score) displayScore = score;

            UiScore.text = $"{displayScore.ToString()}/{scoreManager.quotatOfTheDay}";
            if (displayScore % 25 <= step)
               // MicroShakeCam?.Invoke();

            yield return null;
        }
    }
   

}
