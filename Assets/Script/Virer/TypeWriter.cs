using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public void StartDialogue(List<DialogueLine> lines,Animator animatorSend)
    {
        animator = animatorSend;
        currentLines = lines;
        currentIndex = 0;
        //  employeName pas name
        ShowNextLine();
    }

    public void ShowNextLine()
    {
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
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in line.text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(line.speed);
        }
        animator.SetTrigger("EndTalk");
        isTyping = false;
        
    }

    public void OnClick()
    {
        if (isTyping)
        {
            StopAllCoroutines();
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
        fireManager.FiredLauncher();
    }

}
