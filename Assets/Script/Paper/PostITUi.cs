using UnityEngine;
using UnityEngine.UI;

public class PostItUI : MonoBehaviour
{
    public PoleTask linkedTask;
    [SerializeField] Slider timerBar;

    void Update()
    {
        if (linkedTask == null) return;
        timerBar.value = linkedTask.timeRemaining / linkedTask.timeLimit;
    }
}
