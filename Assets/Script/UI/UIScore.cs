using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class UIScore : MonoBehaviour
{
    [SerializeField] UiManager uiManager;
    [SerializeField] TextMeshProUGUI UiScore;
    [SerializeField] TextMeshProUGUI Uimoney;
    [SerializeField] ScoreManager scoreManager;

    


    private void OnEnable()
    {
        uiManager.ScoreAnim += LaunchAnim;
        uiManager.ScoreReset += ResetScore;
    }

    private void OnDisable()
    {
        uiManager.ScoreAnim -= LaunchAnim;
        uiManager.ScoreReset -= ResetScore;
    }
    public void ResetScore()
    {
        UiScore.text = null;
        Uimoney.text = "Argent:";    

    }
    public void LaunchAnim()
    {
        StartCoroutine(ScoreAnim());
    }

    public IEnumerator ScoreAnim()
    {
        yield return new WaitForSeconds(1.5f);
        float duration = 1f; // durée totale de l’anim
        float t = 0f;

        int score = scoreManager.playerQuotat;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = t / duration;

            int displayScore = Mathf.RoundToInt(Mathf.Lerp(0, score, normalized));

            if (displayScore > score) displayScore = score;

            UiScore.text = $"{displayScore.ToString()}/{scoreManager.quotatOfTheDay}";
            //if (displayScore % 25 <= step)
               // MicroShakeCam?.Invoke();

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
         t = 0f;
         score = scoreManager.playerMoney;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = t / duration;

            int displayScore = Mathf.RoundToInt(Mathf.Lerp(0, score, normalized));

            if (displayScore > score) displayScore = score;

            Uimoney.text = $"Argent: {displayScore.ToString()}";
            //if (displayScore % 25 <= step)
            // MicroShakeCam?.Invoke();

            yield return null;
        }

    }
   

}
