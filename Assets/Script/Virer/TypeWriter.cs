using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TypeWriter : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI dialogueText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] FireManager fireManager;
    [SerializeField] Employe emp;
    private List<DialogueLine> currentLines;
    private int currentIndex = 0;
    private bool isTyping = false;
    public Animator animator;
    private bool isBase = true;
    private enum DialoguePhase { Base, Fired, NotFired }
    private DialoguePhase phase;
    
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    public void StartDialogue(List<DialogueLine> lines, Animator animatorSend)
    {
        phase = DialoguePhase.Base;
        animator = animatorSend;
        currentLines = lines;
        currentIndex = 0;
        ShowNextLine();
    }
    public void ShowNextLine()
    {
        if (currentLines == null)
            return;
        if (currentIndex >= currentLines.Count)
        {
            EndDialogue();
            return;
        }
        StopAllCoroutines();
        StartCoroutine(TypeText(currentLines[currentIndex]));
        currentIndex++;
    }

    private IEnumerator TypeText(DialogueLine line)
    {
        animator.SetTrigger("Talk");
        audioEventDispatcher?.PlayLoopAudio(AudioType.Talk);
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in line.text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(line.speed);
        }
        audioEventDispatcher?.StopLoopAudio();
        animator.SetTrigger("EndTalk");
        isTyping = false;
        
        
    }

    public void OnClick()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            audioEventDispatcher?.StopLoopAudio();
            animator.SetTrigger("EndTalk");
            dialogueText.text = currentLines[currentIndex - 1].text;
            isTyping = false;
        }
        else
        {
            ShowNextLine();
        }
    }

    private void EndDialogue()
    {
        if (fireManager.launch == true)
            return;
        if (phase == DialoguePhase.Base)
        {
            fireManager.ShowChoiceButtons();
        }
        else if (phase == DialoguePhase.Fired)
        {
            fireManager.FiredLauncher();
        }
        else if (phase == DialoguePhase.NotFired)
        {
            fireManager.NotFiredLaunch();
        }
    }
    public void ChoiceFired()
    {
        phase = DialoguePhase.Fired;
        currentIndex = 0;
        currentLines = fireManager.empFiche.employe.firelinesChoice;
        ShowNextLine();
        fireManager.HideChoiceButtons();
        fireManager.DayLaunch = true;
    }

    public void ChoiceNotFired()
    {
        phase = DialoguePhase.NotFired;
        currentIndex = 0;
        currentLines = fireManager.empFiche.employe.notfirelines;
        ShowNextLine();
        fireManager.HideChoiceButtons();
    }
}
